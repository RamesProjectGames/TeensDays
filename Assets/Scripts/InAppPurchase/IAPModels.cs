
using System;

[Serializable]
public enum IAPProductKey
{
    // diamond200,
    // diamond500,
    // diamond1000,
    // diamond2000,
    // diamond5000,
    // diamond10000,
    // boostMaxStats
    
}

[Serializable]
public class IAPPayData
{
    public string Payload;
    public string Store;
    public string TransactionID;
}
[Serializable]
public class IAPPayload
{
    public string json;
    public string signature;
    public IAPPayloadData payloadData;
}
[Serializable]
public class IAPPayloadData
{
    public string orderId;
    public string pacakageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
    public int quantity;
    public bool acknowledged; 
}