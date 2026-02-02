package com.psp.cardservice.repository;

import com.psp.cardservice.model.Transaction;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public interface TransactionRepository extends JpaRepository<Transaction, Long> {
    
    Optional<Transaction> findByPaymentId(String paymentId);
    
    Optional<Transaction> findByStan(String stan);
    
    Optional<Transaction> findByPspPaymentId(String pspPaymentId);
    
    boolean existsByPaymentId(String paymentId);
}
