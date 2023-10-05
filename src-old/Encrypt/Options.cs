namespace ImageFunctions.Encrypt
{
	public class Options
	{
		public bool DoDecryption = false;
		public byte[] Password = null;
		public string UserPassword = null;
		public byte[] IVBytes = Encryptor.DefaultIV;
		public byte[] SaltBytes = Encryptor.DefaultSalt;
		public bool TreatPassAsRaw = false;
		public bool TestMode = false;
		public int PasswordIterations = Encryptor.DefaultIterations;
	}
}