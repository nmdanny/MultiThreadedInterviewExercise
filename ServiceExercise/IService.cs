using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionPool {
    public interface IService {
        void sendRequest(Request request);
        void notifyFinishedLoading();
        int getSummary();
    }
}
