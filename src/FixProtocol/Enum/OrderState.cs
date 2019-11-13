namespace Intelligences.FixProtocol.Enum
{
    public enum OrderState
    {
        /// <summary>
        /// Заявка не отправлена в торговую площадку
        /// </summary>
        None,

        /// <summary>
        /// Заявка в состоянии ожидания на принятие торговой площадкой
        /// </summary>
        Pending,

        /// <summary>
        /// Ошибка регистрации заявки, заявка не принята торговой площадкой
        /// </summary>
        Failed,

        /// <summary>
        /// Заявка активна, еще не исполнена
        /// </summary>
        Active,

        /// <summary>
        /// Заявка частично исполнена
        /// </summary>
        PartialFilled,

        /// <summary>
        /// Заявка исполнена
        /// </summary>
        Filled,

        /// <summary>
        /// Отклонена
        /// </summary>
        Canceled,
    }
}
