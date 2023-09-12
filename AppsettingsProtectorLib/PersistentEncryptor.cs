using System.IO;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector
{
    public interface IEncryptor
    {
        void UnprotectFileAndSave(string filePath);
        void ProtectFileAndSave(string filePath);
        byte[] ProtectFileContents(string filePath);
        UnprotectResult UnprotectFileContents(string filePath);
    }

    public class PersistentEncryptor : IEncryptor
    {
        private readonly IPersistedDataProtector _persistedDataProtector;

        public PersistentEncryptor(IPersistedDataProtector provider)
        {
            _persistedDataProtector = provider;
        }

        public virtual UnprotectResult UnprotectFileContents(string filePath)
        {
            var fileBytes = File.ReadAllBytes(filePath);
            return _persistedDataProtector.DangerousUnprotect(fileBytes);
        }

        public virtual void UnprotectFileAndSave(string filePath)
        {
            var unprotectResult = UnprotectFileContents(filePath);
            File.WriteAllBytes(filePath, unprotectResult.UnprotectedBytes);
        }

        public virtual byte[] ProtectFileContents(string filePath)
        {
            var fileBytes = File.ReadAllBytes(filePath);
            return _persistedDataProtector.Protect(fileBytes);
        }

        public virtual void ProtectFileAndSave(string filePath)
        {
            var protectedBytes = ProtectFileContents(filePath);
            File.WriteAllBytes(filePath, protectedBytes);
        }
    }
}