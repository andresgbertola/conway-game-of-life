namespace GOL.Domain.Enums
{
    /// <summary>
    /// Board State.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// Not finished.
        /// </summary>
        NotFinished = 0,

        /// <summary>
        /// Fade away (dead board).
        /// </summary>
        FadedAway = 1,

        /// <summary>
        /// Oscillatory.
        /// </summary>
        Oscillatory = 2,

        /// <summary>
        /// Stable.
        /// </summary>
        Stable = 3,

        /// <summary>
        /// Infinite.
        /// </summary>
        Infinite = 4
    }
}
