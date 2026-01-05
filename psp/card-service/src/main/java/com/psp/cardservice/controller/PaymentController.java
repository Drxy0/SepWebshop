package com.psp.cardservice.controller;

import com.psp.cardservice.dto.*;
import com.psp.cardservice.model.Transaction;
import com.psp.cardservice.service.PaymentService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/payment")
@RequiredArgsConstructor
@CrossOrigin(origins = "*")
public class PaymentController {
    
    private final PaymentService paymentService;
    
    /**
     * Initialize payment (called by PSP)
     * Returns payment URL and payment ID
     */
    @PostMapping("/init")
    public ResponseEntity<PaymentInitResponse> initializePayment(
            @Valid @RequestBody PaymentInitRequest request) {
        
        PaymentInitResponse response = paymentService.initializePayment(request);
        
        if ("ERROR".equals(response.getStatus())) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(response);
        }
        
        return ResponseEntity.ok(response);
    }
    
    /**
     * Process card payment
     * This endpoint is called after user submits card details on payment form
     */
    @PostMapping("/process")
    public ResponseEntity<CardPaymentResponse> processPayment(
            @Valid @RequestBody CardPaymentRequest request) {
        
        CardPaymentResponse response = paymentService.processCardPayment(request);
        
        if ("ERROR".equals(response.getStatus())) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(response);
        }
        
        if ("FAILED".equals(response.getStatus())) {
            return ResponseEntity.status(HttpStatus.PAYMENT_REQUIRED).body(response);
        }
        
        return ResponseEntity.ok(response);
    }
    
    /**
     * Get transaction status
     */
    @GetMapping("/transaction/{paymentId}")
    public ResponseEntity<?> getTransaction(@PathVariable String paymentId) {
        Transaction transaction = paymentService.getTransaction(paymentId);
        
        if (transaction == null) {
            return ResponseEntity.status(HttpStatus.NOT_FOUND)
                .body("Transaction not found");
        }
        
        return ResponseEntity.ok(transaction);
    }
    
    /**
     * Health check endpoint
     */
    @GetMapping("/health")
    public ResponseEntity<String> health() {
        return ResponseEntity.ok("Card Service is running");
    }
}
