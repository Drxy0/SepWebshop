package com.psp.cardservice.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class BankInitPaymentResponse {
    private String paymentRequestId;
    private String paymentUrl;
}
