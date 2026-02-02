package com.psp.cardservice.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class BankInitPaymentRequest {
    private String merchantId;
    private String pspPaymentId;
    private Double amount;
    private String currency;
    private String stan;
    private Long pspTimestamp;
}
