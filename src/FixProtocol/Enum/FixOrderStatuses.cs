namespace Intelligences.FixProtocol.Enum
{
    /// <summary>
    /// Список статусов заявки
    /// </summary>
    internal enum FixOrderStatuses
    {
        /// <summary>
        /// Нет статуса (заявка еще не была передана в торговую систему)
        /// </summary>
        None,

        /// <summary>
        /// Заявка активна
        /// </summary>
        Active,

        /// <summary>
        /// Заявка была отменена торговой системой
        /// </summary>
        Canceled,

        /// <summary>
        /// Заявка полностью исполнена торговой системой
        /// </summary>
        Filled,

        /// <summary>
        /// Заявка частично исполнена
        /// </summary>
        PartiallyFilled,
    }
}
