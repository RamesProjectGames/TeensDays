
using System;

[Serializable]
public enum IAPProductKey
{
    diamond10,
    diamond25,
    diamond70,
    diamond150,
    diamond320,
    diamond850
    
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