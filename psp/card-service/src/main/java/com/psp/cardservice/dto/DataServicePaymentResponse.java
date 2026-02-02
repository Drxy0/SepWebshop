package com.psp.cardservice.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class DataServicePaymentResponse {
    private String id;
    private String merchantId;
    private String merchantPassword;
    private Double amount;
    private String currency;
    private String merchantOrderId;
    private Long merchantTimestamp;
}
