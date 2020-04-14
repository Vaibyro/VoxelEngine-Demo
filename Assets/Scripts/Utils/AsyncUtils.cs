using System;
using System.Threading.Tasks;

namespace VoxelEngine {
    public static class AsyncUtils {
        private const int Delay = 200;
        
        /// <summary>
        /// Method to wait until a condition after an "await" statement.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task WaitUntil(Func<bool> predicate) {
            while (!predicate()) {
                await Task.Delay(Delay);
            }
        }
    }
}