package com.psp.cardservice.service;

import com.psp.cardservice.dto.*;
import com.psp.cardservice.model.Transaction;
import com.psp.cardservice.repository.TransactionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.Random;
import java.util.UUID;
import java.util.concurrent.atomic.AtomicLong;

@Service
@RequiredArgsConstructor
public class PaymentService {
    
    private final TransactionRepository transactionRepository;
    private final CardValidationService cardValidationService;
    private final Random random = new Random();
    private final AtomicLong paymentIdCounter = new AtomicLong(1);
    
    @Value("${acquirer.merchant.id}")
    private String acquirerMerchantId;
    
    @Value("${server.port}")
    private String serverPort;
    
    /**
     * Initialize payment and return payment URL and ID
     */
    public PaymentInitResponse initializePayment(PaymentInitRequest request) {
        // Validate merchant ID
        if (!acquirerMerchantId.equals(request.getMerchantId())) {
            return PaymentInitResponse.builder()
                .status("ERROR")
                .message("Invalid merchant ID")
                .build();
        }
        
        // Check if STAN already exists
        if (transactionRepository.findByStan(request.getStan()).isPresent()) {
            return PaymentInitResponse.builder()
                .status("ERROR")
                .message("Duplicate STAN")
                .build();
        }
        
        // Generate payment ID (sequential: 1, 2, 3, ...)
        String paymentId = String.valueOf(paymentIdCounter.getAndIncrement());
        
        // Create transaction record
        Transaction transaction = Transaction.builder()
            .paymentId(paymentId)
            .merchantId(request.getMerchantId())
            .amount(request.getAmount())
            .currency(request.getCurrency())
            .stan(request.getStan())
            .status("PENDING")
            .createdAt(LocalDateTime.now())
            .pspTimestamp(request.getPspTimestamp())
            .build();
        
        transactionRepository.save(transaction);
        
        // Generate payment URL
        String paymentUrl = String.format("http://localhost:%s/api/payment/form/%s", 
            serverPort, paymentId);
        
        return PaymentInitResponse.builder()
            .paymentId(paymentId)
            .paymentUrl(paymentUrl)
            .status("SUCCESS")
            .message("Payment initialized successfully")
            .build();
    }
    
    /**
     * Process card payment
     */
    public CardPaymentResponse processCardPayment(CardPaymentRequest request) {
        // Find transaction by payment ID
        Transaction transaction = transactionRepository.findByPaymentId(request.getPaymentId())
            .orElse(null);
        
        if (transaction == null) {
            return CardPaymentResponse.builder()
                .status("ERROR")
                .message("Payment ID not found")
                .build();
        }
        
        // Check if transaction is still pending
        if (!"PENDING".equals(transaction.getStatus())) {
            return CardPaymentResponse.builder()
                .status("ERROR")
                .message("Transaction already processed")
                .stan(transaction.getStan())
                .build();
        }
        
        // Validate PAN using Luhn algorithm
        if (!cardValidationService.validatePAN(request.getPan())) {
            transaction.setStatus("FAILED");
            transaction.setCompletedAt(LocalDateTime.now());
            transactionRepository.save(transaction);
            
            return CardPaymentResponse.builder()
                .status("FAILED")
                .message("Invalid card number (Luhn check failed)")
                .stan(transaction.getStan())
                .build();
        }
        
        // Validate expiry date
        if (!cardValidationService.validateExpiryDate(request.getExpiryDate())) {
            transaction.setStatus("FAILED");
            transaction.setCompletedAt(LocalDateTime.now());
            transactionRepository.save(transaction);
            
            return CardPaymentResponse.builder()
                .status("FAILED")
                .message("Card expired or invalid expiry date")
                .stan(transaction.getStan())
                .build();
        }
        
        // Simulate checking if customer has sufficient funds
        // In real scenario, this would communicate with issuer bank
        boolean hasSufficientFunds = simulateBalanceCheck(request.getPan(), transaction.getAmount());
        
        if (!hasSufficientFunds) {
            transaction.setStatus("FAILED");
            transaction.setCompletedAt(LocalDateTime.now());
            transactionRepository.save(transaction);
            
            return CardPaymentResponse.builder()
                .status("FAILED")
                .message("Insufficient funds")
                .stan(transaction.getStan())
                .build();
        }
        
        // Reserve funds and update transaction
        String globalTransactionId = generateGlobalTransactionId();
        Long acquirerTimestamp = System.currentTimeMillis();
        
        transaction.setStatus("SUCCESS");
        transaction.setGlobalTransactionId(globalTransactionId);
        transaction.setAcquirerTimestamp(acquirerTimestamp);
        transaction.setPanLastFour(cardValidationService.getLastFourDigits(request.getPan()));
        transaction.setCardHolderName(request.getCardHolderName());
        transaction.setCompletedAt(LocalDateTime.now());
        
        transactionRepository.save(transaction);
        
        return CardPaymentResponse.builder()
            .status("SUCCESS")
            .message("Payment processed successfully")
            .globalTransactionId(globalTransactionId)
            .acquirerTimestamp(acquirerTimestamp)
            .stan(transaction.getStan())
            .build();
    }
    
    /**
     * Get transaction by payment ID
     */
    public Transaction getTransaction(String paymentId) {
        return transactionRepository.findByPaymentId(paymentId).orElse(null);
    }
    
    /**
     * Simulate balance check (in real scenario, would call issuer bank)
     */
    private boolean simulateBalanceCheck(String pan, Double amount) {
        // For demonstration: Cards ending in even number have sufficient funds
        // In production, this would make an actual call to the issuer bank
        String lastDigit = pan.substring(pan.length() - 1);
        int digit = Integer.parseInt(lastDigit);
        
        // 80% chance of success for testing purposes
        return random.nextInt(100) < 80;
    }
    
    /**
     * Generate global transaction ID
     */
    private String generateGlobalTransactionId() {
        return "GTX-" + System.currentTimeMillis() + "-" + random.nextInt(10000);
    }
}
