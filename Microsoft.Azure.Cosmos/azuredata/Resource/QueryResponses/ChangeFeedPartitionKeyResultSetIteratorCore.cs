//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Azure.Cosmos
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Documents;

    /// <summary>
    /// Cosmos Change Feed Iterator for a particular Partition Key Range
    /// </summary>
    internal class ChangeFeedPartitionKeyResultSetIteratorCore : FeedIterator
    {
        private readonly CosmosClientContext clientContext;
        private readonly ContainerCore container;
        private readonly ChangeFeedRequestOptions changeFeedOptions;
        private string continuationToken;
        private string partitionKeyRangeId;
        private bool hasMoreResultsInternal;

        internal ChangeFeedPartitionKeyResultSetIteratorCore(
            CosmosClientContext clientContext,
            ContainerCore container,
            string partitionKeyRangeId,
            string continuationToken,
            int? maxItemCount,
            ChangeFeedRequestOptions options)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (partitionKeyRangeId == null)
            {
                throw new ArgumentNullException(nameof(partitionKeyRangeId));
            }

            this.clientContext = clientContext;
            this.container = container;
            this.changeFeedOptions = options;
            this.MaxItemCount = maxItemCount;
            this.continuationToken = continuationToken;
            this.partitionKeyRangeId = partitionKeyRangeId;
        }

        /// <summary>
        /// Gets or sets the maximum number of items to be returned in the enumeration operation in the Azure Cosmos DB service.
        /// </summary>
        public int? MaxItemCount { get; set; }

        public override bool HasMoreResults => this.hasMoreResultsInternal;

        /// <summary>
        /// Get the next set of results from the cosmos service
        /// </summary>
        /// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>A change feed response from cosmos service</returns>
        public override Task<Response> ReadNextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return this.NextResultSetDelegateAsync(this.continuationToken, this.partitionKeyRangeId, this.MaxItemCount, this.changeFeedOptions, cancellationToken)
                .ContinueWith(task =>
                {
                    Response response = task.Result;
                    // Change Feed uses ETAG
                    ResponseMessage responseMessage = response as ResponseMessage;
                    this.continuationToken = responseMessage.CosmosHeaders.ETag;
                    this.hasMoreResultsInternal = response.Status != (int)HttpStatusCode.NotModified;
                    responseMessage.CosmosHeaders.ContinuationToken = this.continuationToken;
                    return response;
                }, cancellationToken);
        }

        private Task<Response> NextResultSetDelegateAsync(
            string continuationToken,
            string partitionKeyRangeId,
            int? maxItemCount,
            ChangeFeedRequestOptions options,
            CancellationToken cancellationToken)
        {
            Uri resourceUri = this.container.LinkUri;
            return this.clientContext.ProcessResourceOperationStreamAsync(
                cosmosContainerCore: this.container,
                resourceUri: resourceUri,
                resourceType: ResourceType.Document,
                operationType: OperationType.ReadFeed,
                requestOptions: options,
                requestEnricher: request =>
                {
                    ChangeFeedRequestOptions.FillContinuationToken(request, continuationToken);
                    ChangeFeedRequestOptions.FillMaxItemCount(request, maxItemCount);
                    ChangeFeedRequestOptions.FillPartitionKeyRangeId(request, partitionKeyRangeId);
                },
                partitionKey: null,
                streamPayload: null,
                cancellationToken: cancellationToken);
        }
    }
}