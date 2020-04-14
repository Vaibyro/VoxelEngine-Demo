using System;
using System.Threading.Tasks;

namespace VoxelEngine {
    public static class AsyncUtils {
        private const int Delay = 200;
        
        public static async Task WaitUntil(Func<bool> predicate) {
            while (!predicate()) {
                await Task.Delay(Delay);
            }
        }
    }
}