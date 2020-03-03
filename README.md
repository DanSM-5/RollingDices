# RollingDices
Game application to simulate the rolling process of dices. 
The demo app can execute the process in 3 ways:

* Synchronous execution
  * It block the UI
  * Can't be cancel
  * Progress bar doesn't update
* Synchronous non-blocking
  * It doesn't block UI
  * It calls an async method
  * Doesn't take adventage of asynchronous execution
* Asynchronous
  * Doesn't block UI
  * Executes all the process in parallel
  * It is a lot more faster
  
The library can be reused with any other UI or .Net project
