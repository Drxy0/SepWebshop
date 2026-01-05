package com.psp.cardservice.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class CardPaymentResponse {
    
    private String status;
    private String message;
    private String globalTransactionId;
    private Long acquirerTimestamp;
    private String stan;
}
