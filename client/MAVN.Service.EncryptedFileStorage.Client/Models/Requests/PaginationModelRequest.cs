namespace MAVN.Service.EncryptedFileStorage.Client.Models.Requests
{
    /// <summary>
    /// Hold information about the Current page and the amount of items on each page
    /// </summary>
    public class PaginationModelRequest
    {
        /// <summary>
        /// The Current Page
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// The amount of items that the page holds
        /// </summary>
        public int PageSize { get; set; }
    }
}
