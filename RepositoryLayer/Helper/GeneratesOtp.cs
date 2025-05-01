using System;

namespace RepositoryLayer.Helper
{
    public class GeneratesOtp
    {
        private readonly Random _random;

        public GeneratesOtp()
        {
            _random = new Random();
        }

        public string GenerateOtp(int length = 6)
        {
            string otp = "";
            for (int i = 0; i < length; i++)
            {
                otp += _random.Next(0, 10); 
            }
            return otp;
        }
    }
}
