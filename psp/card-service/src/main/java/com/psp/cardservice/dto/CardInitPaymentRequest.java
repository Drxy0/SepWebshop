package com.psp.cardservice.dto;

import jakarta.validation.constraints.NotBlank;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CardInitPaymentRequest {
    @NotBlank(message = "Merchant order ID is required")
    private String merchantOrderId;
}
