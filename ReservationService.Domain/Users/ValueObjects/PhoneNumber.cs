namespace ReservationService.Domain.Users.ValueObjects
{
    public record PhoneNumber
    {
        public string Number {  get;}
        
        private PhoneNumber(string number)
        {
            Number = number;
        }

        public static PhoneNumber Create(string number) 
        {
            var cleaned = new string(number.Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '(' && c != ')').ToArray());

            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Phone number cannot be empty");

            if (!cleaned.All(c => char.IsDigit(c) || c == '+'))
                throw new ArgumentException("Phone number can only contain digits and optional +");

            return new PhoneNumber(number);
        }
    }
}
