namespace Intelligences.FixProtocol.Enum
{
    /// <summary>
    /// Fix order states
    /// </summary>
    public enum FixOrderState
    {
        /// <summary>
        /// None (Order not sent to the broker)
        /// </summary>
        None,

        /// <summary>
        /// Order pending registration (Pending New: A)
        /// </summary>
        PendingRegistration,

        /// <summary>
        /// New (Order accepted and active)
        /// </summary>
        New,

        /// <summary>
        /// Order partially filled
        /// </summary>
        PartialFilled,

        /// <summary>
        /// Order filled
        /// </summary>
        Filled,

        /// <summary>
        /// Order pending cancel
        /// </summary>
        PendingCancel,
        
        /// <summary>
        /// Order canceled
        /// </summary>
        Canceled,

        /// <summary>
        /// Order rejected
        /// </summary>
        Rejected,

        /// <summary>
        /// Order Suspended
        /// </summary>
        Suspended,
    }
}
