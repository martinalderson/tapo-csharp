using System.Security.Cryptography;
using System.Text;

namespace TapoCSharp;

/// <summary>
/// KLAP protocol cipher for encryption/decryption and key derivation.
/// </summary>
internal class KlapCipher
{
    private readonly byte[] _key;           // AES key (16 bytes)
    private readonly byte[] _ivBase;        // IV base (12 bytes)
    private readonly byte[] _sig;           // Signature key (28 bytes)
    private int _sequence;                  // Current sequence number

    public KlapCipher(byte[] localSeed, byte[] remoteSeed, byte[] authHash, int initialSequence)
    {
        var localHash = CombineArrays(localSeed, remoteSeed, authHash);
        
        _key = DeriveKey(localHash);
        (_ivBase, _sequence) = DeriveIV(localHash);
        _sig = DeriveSignature(localHash);
        
        // Use the provided initial sequence if it's valid
        if (initialSequence != 0)
            _sequence = initialSequence;
    }

    /// <summary>
    /// Encrypts data and returns signature + encrypted data with sequence number.
    /// </summary>
    public (byte[] payload, int sequence) Encrypt(string data)
    {
        _sequence++;
        
        var sequenceBytes = BitConverter.GetBytes(_sequence);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sequenceBytes); // Convert to big-endian
            
        // Create IV for this request: ivBase (12 bytes) + sequence (4 bytes)
        var iv = new byte[16];
        Array.Copy(_ivBase, 0, iv, 0, 12);
        Array.Copy(sequenceBytes, 0, iv, 12, 4);
        
        // Encrypt the JSON data
        var encryptedData = EncryptAes(data, _key, iv);
        
        // Create signature: SHA256(sig + sequenceBytes + encryptedData)
        var signatureInput = CombineArrays(_sig, sequenceBytes, encryptedData);
        var signature = Sha256(signatureInput);
        
        // Final payload: signature (32 bytes) + encrypted data
        var payload = CombineArrays(signature, encryptedData);
        
        return (payload, _sequence);
    }

    /// <summary>
    /// Decrypts response data.
    /// </summary>
    public string Decrypt(int sequence, byte[] encryptedData)
    {
        var sequenceBytes = BitConverter.GetBytes(sequence);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sequenceBytes); // Convert to big-endian
            
        // Create IV: ivBase (12 bytes) + sequence (4 bytes)
        var iv = new byte[16];
        Array.Copy(_ivBase, 0, iv, 0, 12);
        Array.Copy(sequenceBytes, 0, iv, 12, 4);
        
        return DecryptAes(encryptedData, _key, iv);
    }

    /// <summary>
    /// Derives the AES encryption key.
    /// key = SHA256("lsk" + localSeed + remoteSeed + authHash)[0..16]
    /// </summary>
    public static byte[] DeriveKey(byte[] localHash)
    {
        var keyInput = CombineArrays(Encoding.UTF8.GetBytes("lsk"), localHash);
        var keyHash = Sha256(keyInput);
        var key = new byte[16];
        Array.Copy(keyHash, 0, key, 0, 16);
        return key;
    }

    /// <summary>
    /// Derives the IV base and initial sequence number.
    /// iv_hash = SHA256("iv" + localSeed + remoteSeed + authHash)
    /// iv = iv_hash[0..12], initial_seq = int32_from_bytes(iv_hash[28..32])
    /// </summary>
    public static (byte[] ivBase, int initialSequence) DeriveIV(byte[] localHash)
    {
        var ivInput = CombineArrays(Encoding.UTF8.GetBytes("iv"), localHash);
        var ivHash = Sha256(ivInput);
        
        var ivBase = new byte[12];
        Array.Copy(ivHash, 0, ivBase, 0, 12);
        
        var seqBytes = new byte[4];
        Array.Copy(ivHash, 28, seqBytes, 0, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(seqBytes); // Convert from big-endian
        var initialSequence = BitConverter.ToInt32(seqBytes, 0);
        
        return (ivBase, initialSequence);
    }

    /// <summary>
    /// Derives the signature key.
    /// sig = SHA256("ldk" + localSeed + remoteSeed + authHash)[0..28]
    /// </summary>
    public static byte[] DeriveSignature(byte[] localHash)
    {
        var sigInput = CombineArrays(Encoding.UTF8.GetBytes("ldk"), localHash);
        var sigHash = Sha256(sigInput);
        var sig = new byte[28];
        Array.Copy(sigHash, 0, sig, 0, 28);
        return sig;
    }

    /// <summary>
    /// Calculates SHA-1 hash.
    /// </summary>
    public static byte[] Sha1(byte[] data)
    {
        using var sha1 = SHA1.Create();
        return sha1.ComputeHash(data);
    }

    /// <summary>
    /// Calculates SHA-256 hash.
    /// </summary>
    public static byte[] Sha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
    }

    /// <summary>
    /// Encrypts data using AES-128-CBC with PKCS7 padding.
    /// </summary>
    private static byte[] EncryptAes(string plaintext, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        return encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
    }

    /// <summary>
    /// Decrypts data using AES-128-CBC with PKCS7 padding.
    /// </summary>
    private static string DecryptAes(byte[] ciphertext, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// Combines multiple byte arrays into one.
    /// </summary>
    private static byte[] CombineArrays(params byte[][] arrays)
    {
        var totalLength = arrays.Sum(arr => arr.Length);
        var result = new byte[totalLength];
        var offset = 0;
        
        foreach (var array in arrays)
        {
            Array.Copy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }
        
        return result;
    }
}