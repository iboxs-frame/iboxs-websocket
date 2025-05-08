<?php
namespace iboxs\websocket\common;

class Encrypt{
    // AES 加密方法
    public static function aesEncrypt($data, $key,$ivString)
    {
        $encrypted = openssl_encrypt($data, 'AES-256-CBC', $key, OPENSSL_RAW_DATA, $ivString);
        return base64_encode($encrypted);
    }

    // AES 解密方法
    public static function aesDecrypt($cipherText, $key, $ivString) {
        $cipherText = base64_decode($cipherText);
        return openssl_decrypt($cipherText, 'AES-256-CBC', $key, OPENSSL_RAW_DATA, $ivString);
    }

    // RSA 公钥加密方法
    public static function rsaEncrypt($data, $publicKey)
    {
        $encrypted = '';
        openssl_public_encrypt($data, $encrypted, $publicKey);
        return base64_encode($encrypted);
    }

    // RSA 私钥解密方法
    public static function rsaDecrypt($encryptedData, $privateKey)
    {
        $decrypted = '';
        $encryptedData = base64_decode($encryptedData);
        openssl_private_decrypt($encryptedData, $decrypted, $privateKey);
        return $decrypted;
    }
}

?>