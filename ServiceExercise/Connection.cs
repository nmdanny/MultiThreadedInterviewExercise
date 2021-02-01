using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionPool {
    public class Connection : IDisposable {

        public async Task<int> runCommandAsync(int value) {
            await Task.Delay(50);
            Console.WriteLine(value + " function has ended");
            if (value % 5 == 0) {
                return 1;
            }
            else {
                return 0;
            }
        }

        public void Dispose() {
            
        }
    }
}
