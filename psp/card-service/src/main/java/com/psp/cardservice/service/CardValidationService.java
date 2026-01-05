package com.psp.cardservice.service;

import org.springframework.stereotype.Service;

import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeParseException;

@Service
public class CardValidationService {
    
    /**
     * Validates card PAN using Luhn algorithm
     */
    public boolean validatePAN(String pan) {
        if (pan == null || pan.isEmpty()) {
            return false;
        }
        
        // Remove any spaces or hyphens
        pan = pan.replaceAll("[\\s-]", "");
        
        // Check if contains only digits and length is between 13-19
        if (!pan.matches("\\d{13,19}")) {
            return false;
        }
        
        return luhnCheck(pan);
    }
    
    /**
     * Luhn algorithm implementation
     */
    private boolean luhnCheck(String cardNumber) {
        int sum = 0;
        boolean alternate = false;
        
        // Process digits from right to left
        for (int i = cardNumber.length() - 1; i >= 0; i--) {
            int digit = Character.getNumericValue(cardNumber.charAt(i));
            
            if (alternate) {
                digit *= 2;
                if (digit > 9) {
                    digit = (digit % 10) + 1;
                }
            }
            
            sum += digit;
            alternate = !alternate;
        }
        
        return (sum % 10 == 0);
    }
    
    /**
     * Validates expiry date in MM/YY format
     */
    public boolean validateExpiryDate(String expiryDate) {
        if (expiryDate == null || !expiryDate.matches("(0[1-9]|1[0-2])/\\d{2}")) {
            return false;
        }
        
        try {
            String[] parts = expiryDate.split("/");
            int month = Integer.parseInt(parts[0]);
            int year = 2000 + Integer.parseInt(parts[1]); // Convert YY to YYYY
            
            LocalDate cardExpiry = LocalDate.of(year, month, 1)
                .plusMonths(1)
                .minusDays(1); // Last day of expiry month
            
            return !cardExpiry.isBefore(LocalDate.now());
        } catch (Exception e) {
            return false;
        }
    }
    
    /**
     * Get card type based on PAN
     */
    public String getCardType(String pan) {
        if (pan == null || pan.isEmpty()) {
            return "UNKNOWN";
        }
        
        pan = pan.replaceAll("[\\s-]", "");
        
        if (pan.startsWith("4")) {
            return "VISA";
        } else if (pan.matches("^5[1-5].*")) {
            return "MASTERCARD";
        } else if (pan.matches("^3[47].*")) {
            return "AMEX";
        } else if (pan.matches("^6(?:011|5).*")) {
            return "DISCOVER";
        } else if (pan.matches("^3(?:0[0-5]|[68]).*")) {
            return "DINERS";
        }
        
        return "UNKNOWN";
    }
    
    /**
     * Mask PAN for security (show only last 4 digits)
     */
    public String maskPAN(String pan) {
        if (pan == null || pan.length() < 4) {
            return "****";
        }
        
        return "**** **** **** " + pan.substring(pan.length() - 4);
    }
    
    /**
     * Get last 4 digits of PAN
     */
    public String getLastFourDigits(String pan) {
        if (pan == null || pan.length() < 4) {
            return "";
        }
        
        return pan.substring(pan.length() - 4);
    }
}
