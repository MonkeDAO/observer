using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Solnet.Rpc;
using Solnet.Rpc.Models;

namespace Observer.Handlers
{
    /// <summary>
    /// Defines common functionality for the different watcher handlers.
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// Handles the decoded instructions.
        /// </summary>
        void Handle(TransactionMetaInfo tx);
        
        /// <summary>
        /// Set the collection provider.
        /// </summary>
        /// <param name="provider">The collection provider.</param>
        void SetProvider(CollectionProvider provider);

        /// <summary>
        /// Set the logger instance.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        void SetLogger(ILogger logger);

        /// <summary>
        /// Set the rpc client instance.
        /// </summary>
        /// <param name="client">The client instance.</param>
        void SetClient(IRpcClient client);
    }
}