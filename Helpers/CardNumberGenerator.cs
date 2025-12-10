using System;

namespace virtupay_corporate.Helpers
{
    /// <summary>
    /// Helper class for generating valid payment card numbers using the Luhn algorithm.
    /// </summary>
    public class CardNumberGenerator
    {
        /// <summary>
        /// Generates a valid 16-digit Mastercard number that passes the Luhn algorithm.
  /// </summary>
        /// <returns>A 16-digit card number string</returns>
        public static string GenerateMastercardNumber()
        {
    // Mastercard starts with 51-55
            string cardNumber = GenerateValidCardNumber("51");
   return cardNumber;
        }

        /// <summary>
        /// Generates a valid card number with the given prefix that passes the Luhn algorithm.
        /// </summary>
        /// <param name="prefix">The starting digits of the card (e.g., "51" for Mastercard)</param>
    /// <returns>A 16-digit card number string</returns>
        private static string GenerateValidCardNumber(string prefix)
   {
            Random random = new Random();
          
         // Generate remaining digits (14 more digits needed for a 16-digit card)
    string cardWithoutCheckDigit = prefix;
      for (int i = 0; i < 14; i++)
   {
    cardWithoutCheckDigit += random.Next(0, 10);
      }

    // Calculate the check digit using Luhn algorithm
       int checkDigit = CalculateLuhnCheckDigit(cardWithoutCheckDigit);
            
            return cardWithoutCheckDigit + checkDigit;
        }

   /// <summary>
    /// Calculates the Luhn check digit for a card number.
      /// </summary>
      /// <param name="cardNumberWithoutCheck">The card number without the check digit</param>
        /// <returns>The check digit (0-9)</returns>
 private static int CalculateLuhnCheckDigit(string cardNumberWithoutCheck)
        {
      int sum = 0;
            bool isSecondDigit = false;

            // Process digits from right to left
          for (int i = cardNumberWithoutCheck.Length - 1; i >= 0; i--)
            {
       int digit = int.Parse(cardNumberWithoutCheck[i].ToString());

    if (isSecondDigit)
   {
  digit *= 2;
      if (digit > 9)
       {
     digit -= 9;
         }
         }

                sum += digit;
     isSecondDigit = !isSecondDigit;
     }

         // The check digit is the number that makes the sum a multiple of 10
      int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit;
        }

        /// <summary>
        /// Validates a card number using the Luhn algorithm.
      /// </summary>
  /// <param name="cardNumber">The card number to validate</param>
        /// <returns>True if the card number is valid, false otherwise</returns>
        public static bool ValidateCardNumber(string cardNumber)
     {
            if (string.IsNullOrWhiteSpace(cardNumber) || !cardNumber.All(char.IsDigit) || cardNumber.Length != 16)
     {
        return false;
            }

            int sum = 0;
            bool isSecondDigit = false;

     for (int i = cardNumber.Length - 1; i >= 0; i--)
  {
        int digit = int.Parse(cardNumber[i].ToString());

        if (isSecondDigit)
         {
           digit *= 2;
           if (digit > 9)
              {
            digit -= 9;
     }
              }

        sum += digit;
             isSecondDigit = !isSecondDigit;
    }

   return sum % 10 == 0;
        }
    }
}
