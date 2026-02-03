package com.psp.cardservice.model;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Entity
@Table(name = "transactions")
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Transaction {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(nullable = false, unique = true)
    private String paymentId;
    
    @Column(nullable = false)
    private String merchantId;
    
    @Column(nullable = false)
    private Double amount;
    
    @Column(nullable = false)
    private String currency;
    
    @Column(nullable = false, unique = true)
    private String stan;
    
    private String merchantOrderId;
    
    private String merchantPassword;
    
    private String pspPaymentId;
    
    private Boolean isProcessed;
    
    private String globalTransactionId;
    
    @Column(nullable = false)
    private String status; // PENDING, SUCCESS, FAILED, ERROR
    
    private String panLastFour; // Only store last 4 digits for security
    
    private String cardHolderName;
    
    @Column(nullable = false)
    private LocalDateTime createdAt;
    
    private LocalDateTime completedAt;
    
    private Long pspTimestamp;
    
    private Long acquirerTimestamp;
}
