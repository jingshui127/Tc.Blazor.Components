namespace Tc.Blazor.Components.Timer
{
    /// <summary>
    /// <para>Times to occur of <see cref="AdvancedTimer"/> ticks</para>
    /// <para>计时器触发的次数</para>
    /// </summary>
    public record Times
    {
        /// <summary>
        /// <para>Occurrence count</para>
        /// <para>发生次数</para>
        /// </summary>
        public ulong Count { get; init; }

        private Times() { }

        /// <summary>
        /// <para>Should occur only once</para>
        /// <para>只发生一次</para>
        /// </summary>
        /// <returns></returns>
        public static Times Once() => new Times { Count = 1 };

        /// <summary>
        /// <para>Should occur until stopped</para>
        /// <para>一直发生直到停止</para>
        /// </summary>
        /// <returns></returns>
        public static Times Infinite() => new Times { Count = ulong.MaxValue };

        /// <summary>
        /// <para>Should occur exactly to the given number of times</para>
        /// <para>发生指定次数</para>
        /// </summary>
        /// <param name="count">
        /// <para>N occurrence</para>
        /// <para>发生次数</para>
        /// </param>
        /// <returns></returns>
        public static Times Exactly(ulong count) => new Times { Count = count };
    }
}