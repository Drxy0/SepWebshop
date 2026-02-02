package com.psp.cardservice.controller;

import com.psp.cardservice.dto.BankUpdatePaymentRequest;
import com.psp.cardservice.dto.CardInitPaymentRequest;
import com.psp.cardservice.dto.CardInitPaymentResponse;
import com.psp.cardservice.service.CardPaymentService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/ca")
@RequiredArgsConstructor
@CrossOrigin(origins = "*")
public class CardServiceController {
    
    private final CardPaymentService cardPaymentService;
    
    /**
     * Initialize card payment - similar to QR service
     * POST /ca/Payment/init
     */
    @PostMapping("/Payment/init")
    public ResponseEntity<?> initializePayment(@Valid @RequestBody CardInitPaymentRequest request) {
        try {
            CardInitPaymentResponse response = cardPaymentService.initializeCardPayment(request);
            return ResponseEntity.ok(response);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST)
                .body(new ErrorResponse(e.getMessage()));
        }
    }
    
    /**
     * Update payment status from bank callback
     * PUT /ca/Payment/bank/update/{paymentId}
     */
    @PutMapping("/Payment/bank/update/{paymentId}")
    public ResponseEntity<?> updatePaymentFromBank(
            @PathVariable String paymentId,
            @Valid @RequestBody BankUpdatePaymentRequest request) {
        try {
            cardPaymentService.updatePaymentFromBank(paymentId, request);
            return ResponseEntity.ok(new SuccessResponse("Payment updated successfully"));
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST)
                .body(new ErrorResponse(e.getMessage()));
        }
    }
    
    /**
     * Health check / ping endpoint
     * GET /ca/test/ping
     */
    @GetMapping("/test/ping")
    public ResponseEntity<String> ping() {
        return ResponseEntity.ok("Pong Card Service");
    }
    
    // Helper classes for responses
    private record ErrorResponse(String message) {}
    private record SuccessResponse(String message) {}
}
