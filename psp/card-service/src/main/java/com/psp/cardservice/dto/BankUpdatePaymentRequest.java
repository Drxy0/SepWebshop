package com.psp.cardservice.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class BankUpdatePaymentRequest {
    private String status;
    private String globalTransactionId;
    private Long acquirerTimestamp;
}
