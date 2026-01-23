import { CardBrand } from "./card-payment.models";

export function detectCardBrand(cardNumber: string): CardBrand {
  const digits = cardNumber.replace(/\s+/g, '');
  if (/^4\d*/.test(digits)) return 'VISA';
  if (/^(5[1-5]|2[2-7])/.test(digits)) return 'MASTERCARD';
  return 'UNKNOWN';
}

export function isExpiryValid(expiry: string): boolean {
  if (!/^\d{2}\/\d{2}$/.test(expiry)) return false;
  const [month, year] = expiry.split('/').map(Number);
  if (month < 1 || month > 12) return false;
  const now = new Date();
  const expiryDate = new Date(2000 + year, month);
  return expiryDate > now;
}

export function isValidLuhn(cardNumber: string): boolean {
  const digits = cardNumber.replace(/\s+/g, '');
  let sum = 0;
  let alternate = false;
  for (let i = digits.length - 1; i >= 0; i--) {
    let n = Number(digits[i]);
    if (alternate) {
      n *= 2;
      if (n > 9) n -= 9;
    }
    sum += n;
    alternate = !alternate;
  }
  return sum % 10 === 0;
}