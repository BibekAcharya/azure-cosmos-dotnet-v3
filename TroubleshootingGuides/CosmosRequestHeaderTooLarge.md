## CosmosRequestHeaderTooLarge

| Http Status Code | Name | Category |
|---|---|---|
|400|CosmosRequestHeaderTooLarge|Service|

## Description
The size of the header has grown to large and is exceeding the maximum allowed size. It's always recommended to use the latest SDK. Make sure to use at least version 3.x or 2.x, which adds header size tracing to the exception message.

## Troubleshooting steps

### 1. Session Token too large

#### Cause:
The 400 bad request is happening on point operations where the continuation token is not being used. The exception started without making any changes to the application.

#### Symptoms:
The session token grows as the number of partitions increase in the container. The numbers of partition increase as the amount of data increase or if the thoughput is increased.

#### Temprorary mitigation: 
Restart the application will reset all the session token. This session token will eventually grow back to the previous size that causes the issue.

#### Solution:
1. Follow the performance tips and convert the application to Direct + TCP connection mode. Direct + TCP does not have the header size restriction like HTTP does which avoids this issue. Make sure to use the latest SDK version which has a fix for query opertaions when the service interop is not available.
2. If the application cannot be converted to Direct + TCP and the session token is the cause, then mitigation can be done by changing the client consistency level. The session token is only used for session consistency which is the default for Cosmos DB. Any other consistency level will not use the session token.
3. If the application cannot be converted to Direct + TCP and the session token is the cause, then mitigation can be done by changing the client consistency level. The session token is only used for session consistency which is the default for Cosmos DB. Any other consistency level will not use the session token.

### 2. Continuation token too large

#### Cause:
The 400 bad request is happening on query operations where the continuation token is being passed in.

#### Symptoms:
The continuation token has grown to large. Different queries will have different continuation token sizes.
    
#### Solution:
1. Follow the performance tips and convert the application to Direct + TCP connection mode. Direct + TCP does not have the header size restriction like HTTP does which avoids this issue.
2. If the application cannot be converted to Direct + TCP and the continuation token is the cause, then try setting the ResponseContinuationTokenLimitInKb option. The option can be found in the FeedOptions for v2 or the QueryRequestOptions in v3.
3. If the application cannot be converted to Direct + TCP and the session token is the cause, then mitigation can be done by changing the client consistency level. The session token is only used for session consistency which is the default for Cosmos DB. Any other consistency level will not use the session token.