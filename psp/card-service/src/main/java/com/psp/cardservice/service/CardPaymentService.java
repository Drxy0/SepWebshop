package com.psp.cardservice.service;

import com.psp.cardservice.dto.*;
import com.psp.cardservice.model.Transaction;
import com.psp.cardservice.repository.TransactionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.nio.charset.StandardCharsets;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.time.Instant;
import java.time.LocalDateTime;
import java.util.Base64;
import java.util.Optional;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class CardPaymentService {
    
    private final TransactionRepository transactionRepository;
    private final RestTemplate restTemplate;
    
    @Value("${data.service.url}")
    private String dataServiceUrl;
    
    @Value("${bank.service.url}")
    private String bankServiceUrl;
    
    @Value("${webshop.order.update.url}")
    private String webshopOrderUpdateUrl;
    
    @Value("${psp.id}")
    private String pspId;
    
    @Value("${psp.hmac.key}")
    private String pspHmacKey;
    
    @Value("${bank.id}")
    private String bankId;
    
    @Value("${bank.password}")
    private String bankPassword;
    
    /**
     * Initialize card payment - similar to QR service flow
     */
    public CardInitPaymentResponse initializeCardPayment(CardInitPaymentRequest request) throws Exception {
        // Step 1: Get payment data from DataService
        String url = dataServiceUrl + "/Payments/" + request.getMerchantOrderId();
        
        ResponseEntity<DataServicePaymentResponse> response = restTemplate.getForEntity(
            url, 
            DataServicePaymentResponse.class
        );
        
        if (!response.getStatusCode().is2xxSuccessful() || response.getBody() == null) {
            throw new Exception("Payment details not found in DataService.");
        }
        
        DataServicePaymentResponse paymentData = response.getBody();
        
        // Step 2: Save payment to local database
        Transaction transaction = Transaction.builder()
            .pspPaymentId(paymentData.getId())
            .merchantId(paymentData.getMerchantId())
            .merchantPassword(paymentData.getMerchantPassword())
            .merchantOrderId(paymentData.getMerchantOrderId())
            .amount(paymentData.getAmount())
            .currency(paymentData.getCurrency())
            .stan(paymentData.getMerchantOrderId())
            .pspTimestamp(paymentData.getMerchantTimestamp())
            .isProcessed(false)
            .status("PENDING")
            .createdAt(LocalDateTime.now())
            .build();
        
        // Check if already exists
        Optional<Transaction> existing = transactionRepository.findByStan(paymentData.getMerchantOrderId());
        if (existing.isPresent()) {
            throw new Exception("Payment already initialized with this merchant order ID");
        }
        
        transaction = transactionRepository.save(transaction);
        
        // Step 3: Call Bank init endpoint
        BankInitPaymentRequest bankRequest = new BankInitPaymentRequest(
            bankId,
            paymentData.getId(),
            paymentData.getAmount(),
            paymentData.getCurrency(),
            paymentData.getMerchantOrderId(),
            paymentData.getMerchantTimestamp()
        );
        
        BankInitPaymentResponse bankResponse = callBankInit(bankRequest);
        
        // Step 4: Return bank URL to redirect user
        return CardInitPaymentResponse.builder()
            .bankUrl(bankResponse.getPaymentUrl())
            .build();
    }
    
    /**
     * Call Bank init endpoint with HMAC signature
     */
    private BankInitPaymentResponse callBankInit(BankInitPaymentRequest bankRequest) throws Exception {
        String url = bankServiceUrl + "/Payments/init";
        
        Instant timestamp = Instant.now();
        
        // Build payload for HMAC
        String payload = String.format("%s|%.2f|%s|%s|%s",
            bankRequest.getMerchantId(),
            bankRequest.getAmount(),
            bankRequest.getCurrency(),
            bankRequest.getStan(),
            timestamp.toString()
        );
        
        // Generate HMAC signature
        String signature = generateHmac(payload, pspHmacKey);
        
        // Create headers
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        headers.set("PspID", pspId);
        headers.set("Signature", signature);
        headers.set("Timestamp", timestamp.toString());
        headers.set("IsQrPayment", "false");
        
        HttpEntity<BankInitPaymentRequest> entity = new HttpEntity<>(bankRequest, headers);
        
        ResponseEntity<BankInitPaymentResponse> response = restTemplate.exchange(
            url,
            HttpMethod.POST,
            entity,
            BankInitPaymentResponse.class
        );
        
        if (!response.getStatusCode().is2xxSuccessful() || response.getBody() == null) {
            throw new Exception("Failed to initialize payment with bank");
        }
        
        return response.getBody();
    }
    
    /**
     * Update payment status from bank callback
     */
    public void updatePaymentFromBank(String paymentId, BankUpdatePaymentRequest request) throws Exception {
        Optional<Transaction> optTransaction = transactionRepository.findByPspPaymentId(paymentId);
        
        if (optTransaction.isEmpty()) {
            throw new Exception("Payment not found");
        }
        
        Transaction transaction = optTransaction.get();
        
        // Update transaction status
        transaction.setStatus(request.getStatus());
        transaction.setGlobalTransactionId(request.getGlobalTransactionId());
        transaction.setAcquirerTimestamp(request.getAcquirerTimestamp());
        transaction.setCompletedAt(LocalDateTime.now());
        transaction.setIsProcessed(true);
        
        transactionRepository.save(transaction);
        
        // Call webshop to update order status
        updateWebshopOrder(transaction.getMerchantOrderId(), request.getStatus());
    }
    
    /**
     * Call webshop to update order status
     */
    private void updateWebshopOrder(String orderId, String status) {
        try {
            String url = webshopOrderUpdateUrl + "/" + orderId;
            
            HttpHeaders headers = new HttpHeaders();
            headers.setContentType(MediaType.APPLICATION_JSON);
            
            String body = String.format("{\"status\": \"%s\"}", status);
            HttpEntity<String> entity = new HttpEntity<>(body, headers);
            
            restTemplate.exchange(url, HttpMethod.PUT, entity, String.class);
        } catch (Exception e) {
            // Log error but don't throw - payment was successful even if webshop update fails
            System.err.println("Failed to update webshop order: " + e.getMessage());
        }
    }
    
    /**
     * Generate HMAC-SHA256 signature
     */
    private String generateHmac(String data, String secret) throws NoSuchAlgorithmException, InvalidKeyException {
        Mac mac = Mac.getInstance("HmacSHA256");
        SecretKeySpec secretKeySpec = new SecretKeySpec(secret.getBytes(StandardCharsets.UTF_8), "HmacSHA256");
        mac.init(secretKeySpec);
        byte[] hash = mac.doFinal(data.getBytes(StandardCharsets.UTF_8));
        return Base64.getEncoder().encodeToString(hash);
    }
}
