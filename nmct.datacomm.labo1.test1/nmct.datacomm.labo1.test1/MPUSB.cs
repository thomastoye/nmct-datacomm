using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Runtime.InteropServices.Marshal;
using System.Runtime.InteropServices;
using System.Windows;

namespace USB_DAQBoard
{


    // COMPILEREN IN 32BITS MODUS TARGET PLATFORM x86
    public   class MPUSB{

    [DllImport("mpusbapi.dll", EntryPoint="_MPUSBGetDLLVersion", CallingConvention=CallingConvention.Cdecl)]
 private  static extern int    MPUSBGetDLLVersion();
  
    [DllImport("mpusbapi.dll", EntryPoint="_MPUSBGetDeviceCount", CallingConvention=CallingConvention.Cdecl)]
    private static  extern UInt16 MPUSBGetDeviceCount(string pVID_PID);

    [DllImport("mpusbapi.dll", EntryPoint="_MPUSBOpen", CallingConvention=CallingConvention.Cdecl)]
    private static  extern  int MPUSBOpen(UInt16 instance , string pVID_PID , string pEP , UInt16 dwDir , UInt16 dwReserved ) ;

   [DllImport("mpusbapi.dll", EntryPoint="_MPUSBClose", CallingConvention=CallingConvention.Cdecl)]
   private static extern int MPUSBClose(int handle) ;

   [DllImport("mpusbapi.dll", EntryPoint="_MPUSBRead", CallingConvention=CallingConvention.Cdecl)]
  private static  extern int MPUSBRead(int handle , int pData , int dwLen ,ref  int pLength , int dwMilliseconds ) ;

  [DllImport("mpusbapi.dll", EntryPoint="_MPUSBWrite", CallingConvention=CallingConvention.Cdecl)]
    private static  extern int MPUSBWrite(int handle , int pData , int dwLen ,ref  int pLength , int dwMilliseconds );

    [DllImport("mpusbapi.dll", EntryPoint="_MPUSBReadInt", CallingConvention=CallingConvention.Cdecl)]
    private static  extern int MPUSBReadInt(int  handle , ref byte[] pData, int dwLen ,ref int pLength , int dwMilliseconds ) ;

        //Functions and Data required from WIN32 API
    private const  int INVALID_HANDLE_VALUE  = -1;
     private const  int  ERROR_INVALID_HANDLE  = 6;

//private Declare Function GetLastError Lib "kernel32" () As Integer

   //Constants for connecting to Microchip FS Demo board
     private const  string vid_pid  = "vid_04d8&pid_000c";
     private const string out_pipe  = "\\MCHP_EP1";
     private const  string  in_pipe  = "\\MCHP_EP1";

     private const short MPUSB_FAIL  = 0;
     private const  short MPUSB_SUCCESS  = 1;

    private const UInt16 MP_WRITE = 0;
    private const UInt16  MP_READ = 1;

  //constanten om de poorten te bepalen
    private const int write_digital_out_port_D  = 0x10;
    private const  int  read_digital_in_port_B  = 0x12;
    private const int write_analog_out  = 0x14;
    private const int read_analog_in  = 0x15;
    private const int  write_digital_out_port_E  = 0x16;
    private const int  read_digital_out_port_D  = 0x18;
    private const int anal_buffer_out  = 0x1B;
 //toevoegen om int16 in te lezen
    private static int read_digital_in_port_E  = 0x0;

 //Declare the IN PIPE and OUT PIPE Public variables
    //Private Shared myInPipe, myOutPipe As Integer
    private static int myInPipe  = INVALID_HANDLE_VALUE;
    private static int myOutPipe  = INVALID_HANDLE_VALUE;

   //synclock object
    private static Object packetLock = new Object();


        
//    'Following are the top-level Functionality to access the board

    public static  void Wait(Int32 lMS ){
      //deze routine wacht in milliseconden
        System.Threading.Thread.Sleep(lMS);
    }

   public static int OpenMPUSBDevice() {
      //   int tempPipe;
      int count ;

        // Always open one device only 
       // tempPipe = INVALID_HANDLE_VALUE;
        count = MPUSBGetDeviceCount(vid_pid);

        if (count > 0) {
            myOutPipe = MPUSBOpen(0, vid_pid, out_pipe, MP_WRITE, 0);
            myInPipe = MPUSBOpen(0, vid_pid, in_pipe, MP_READ, 0);

            if ( ( myOutPipe == INVALID_HANDLE_VALUE) || ( myInPipe == INVALID_HANDLE_VALUE)) {
                MessageBox.Show(Convert.ToString(myOutPipe) + Convert.ToString(myInPipe) + "Failed to open data pipes.");
                myOutPipe = myInPipe = INVALID_HANDLE_VALUE;
              return -1;
            }
            return 0;
        }
        else{
            MessageBox.Show("No devices connected.");
            return -1;
        }
    }

 public static void  CloseMPUSBDevice(){
        if (myOutPipe != INVALID_HANDLE_VALUE) {
            MPUSBClose(myOutPipe);
            myOutPipe = INVALID_HANDLE_VALUE;
        }
        if (myInPipe  != INVALID_HANDLE_VALUE ) {
            MPUSBClose(myInPipe);
            myInPipe = INVALID_HANDLE_VALUE;
        }
 }

    public static string GetVersion(){

        byte[] send_buf = new byte[64];
         byte[] receive_buf = new byte[64];
        int recvLength = 0 ;
        int bufLength = 2;

        if ((myOutPipe != INVALID_HANDLE_VALUE) && ( myInPipe != INVALID_HANDLE_VALUE)) {
            recvLength = 4;
            send_buf[0] = 0; //0x0 - READ_VERSION
            send_buf[1] = 2;

            if (SendReceivePacket(ref send_buf, ref bufLength, ref receive_buf,ref  recvLength, 1000, 1000) == 1) {

                if ((recvLength != 4) || (receive_buf[0] !=  0)){

                    return "Failed to obtain version information.";
                }
                else
                    return "Demo Version  " + receive_buf[3] + "." +  receive_buf[2] ;

            }   
           else
                return "failed to obtain version information";
        }  
        else
            return "failed to obtain version information";
        
    }

    public static byte ReadDigitalInPortB() {
      byte[] send_buf = new byte[64];
      byte[] receive_buf = new byte[64];
      int   recvLength = 0 ;
      int bufLength = 1;
        recvLength = 2;
        send_buf[0] = read_digital_in_port_B;   //Comando
        if (SendReceivePacket(ref send_buf, ref bufLength, ref receive_buf, ref recvLength, 1000, 1000) == 1) {
           if ((recvLength == 2) && (receive_buf[0] == read_digital_in_port_B)){
                return receive_buf[1];
           }
            else
                throw new Exception("USB Operation Failed : digital in");
        }
        else
            throw new Exception("USB Operation Failed : digital in");
  }

   public static int ReadDigitalOutPortD() {
      byte[] send_buf = new byte[64];
      byte[] receive_buf = new byte[64];
      int   recvLength = 2 ;
      int bufLength = 1;
    
        send_buf[0] = read_digital_out_port_D;   //Comando
         if (SendReceivePacket(ref send_buf, ref bufLength, ref receive_buf,ref  recvLength, 1000, 1000) == 1) {
            if ((recvLength == 2) && (receive_buf[0] == read_digital_out_port_D))
                return (receive_buf[1] | (read_digital_in_port_E << 8));
            else
                throw new Exception("USB Operation Failed : read digital out");
         }
        else
            throw new Exception("USB Operation Failed : read digital out");
        
   }

    public static void WriteDigitalOutPortD(Int16 data){
        byte[] send_buf = new byte[64];
        byte[] receive_buf = new byte[64];
        int  recvLength;

        //poort E
        if ((myOutPipe != INVALID_HANDLE_VALUE) && (myInPipe != INVALID_HANDLE_VALUE))
        {
            recvLength = 1;

            send_buf[0] = write_digital_out_port_E;//&H16 
            send_buf[1] = (byte)(data >> 8);
            read_digital_in_port_E = send_buf[1];
            int bufLength = 3;
            if (SendReceivePacket(ref send_buf, ref bufLength,ref  receive_buf,ref recvLength, 1000, 1000) == 1)
            {

                if ((recvLength != 1) || (receive_buf[0] != write_digital_out_port_E))
                {
                    throw new Exception("Failed to update LED");
                }
            }
        }
        //poort D
        if ((myOutPipe != INVALID_HANDLE_VALUE) && (myInPipe!= INVALID_HANDLE_VALUE)) {
            recvLength = 1;
            send_buf[0] = write_digital_out_port_D;//0x10 
            send_buf[1] =(byte) (data & 0xFF);
            int bufLength = 3;

            if (SendReceivePacket(ref send_buf, ref bufLength,ref  receive_buf,ref  recvLength, 1000, 1000) == 1)
            {
                if ((recvLength != 1) || (receive_buf[0] != write_digital_out_port_D))
                {
                    throw new Exception("Failed to update LED");
                }
            }
        }
      
    }

   public static void WriteAnalogOut(byte kanaal , Int16  data ){
      byte[] send_buf = new byte[64];
      byte[] receive_buf = new byte[64];

        int recvLength = 1;
       int bufLength = 4;
        
        send_buf[0] = write_analog_out; 	//Comando
        send_buf[1] = kanaal  ;//kanaal
          send_buf[2] = (byte)((data >> 8) & 0x3)   ;//Data --> 2 bits mayor 
         send_buf[3] = (byte)(data & 0xFF)  ;//Dato --> 8 bits peso minor, total 10 bits (0-1023)

         if (SendReceivePacket(ref send_buf, ref bufLength, ref receive_buf, ref recvLength, 1000, 1000) == 1)
         {
             if ((recvLength != 1) || (receive_buf[0] != write_analog_out))
             {
                 throw new Exception("USB Operation Failed" + "Function analogout");
             }
         }
         else
             throw new Exception("USB Operation Failed" + "Function analogout");
     
        }
//    buffer meegeven
    public static void  WriteAnalogOut(byte  kanaal, byte[] data){
        byte[] send_buf = new byte[64];
      byte[] receive_buf = new byte[64];
      int bufLength = 64;
   
        if (data.Length < 64) 
            throw new ArgumentException("Size of the buffer must be larger than 64", "data");
   
        int recvLength = 1 ;
      
        send_buf[0]= anal_buffer_out ;//Comando
        send_buf[1] = kanaal;  //kanaal

        //'For index As Integer = 2 To 63
        //'    send_buf(index) = CByte((data(index - 1) >> 8) And &H3)   '//Data --> 2 bits mayor 
        //'    send_buf(index + 1) = CByte(data(index - 1) And &HFF)    '//Dato --> 8 bits peso minor, total 10 bits (0-1023)
        //'Next

        Buffer.BlockCopy(data, 0, send_buf, 2, 62);

        if (SendReceivePacket(ref send_buf, ref bufLength,ref  receive_buf, ref recvLength, 1000, 1000) == 1) {
            if ((recvLength != 1) | (receive_buf[0] != anal_buffer_out))
                throw new Exception("USB Operation Failed" + "Function analogout");
         }
        else
            throw new Exception("USB Operation Failed" + "Function analogout");
        }

    public static int  ReadAnalogIn(byte kanaal ) {
        Int16 iAout;
      byte[] send_buf = new byte[64];
      byte[] receive_buf = new byte[64];
   
        int recvLength  = 4;
        int bufLength = 2;

        send_buf[0] = read_analog_in ;//Comando
        send_buf[1] = kanaal ;//Dato
        if (SendReceivePacket(ref send_buf, ref bufLength, ref receive_buf, ref recvLength, 1000, 1000) == 1) {

            if ((recvLength == 4) && (receive_buf[0] == read_analog_in)) {
                iAout = receive_buf[2];
                iAout =(short) ( iAout << 8);
                iAout = (short)(( iAout) | (short)(receive_buf[3]));
                return iAout;
            }
            else
                throw new Exception("USB Operation Failed Analog in");
        }   
        else
            throw new Exception("USB Operation Failed Analog in");
      
    }


//    //'/////////////////////////////////////////////////////////////////////////////
//    //'//
//    //'// A typical application would send a command to the target device and expect
//    //'// a response.
//    //'// SendReceivePacket is a wrapper function that facilitates the
//    //'// send command / read response paradigm
//    //'//
//    //'// SendData - pointer to data to be sent
//    //'// SendLength - length of data to be sent
//    //'// ReceiveData - Points to the buffer that receives the data read from the call
//    //'// ReceiveLength - Points to the number of bytes read
//    //'// SendDelay - time-out value for MPUSBWrite operation in milliseconds
//    //'// ReceiveDelay - time-out value for MPUSBRead operation in milliseconds
//    //'//

    private static int SendReceivePacket( ref  byte[] sendData,  ref int sendLength ,ref  byte[] receiveData,ref int receiveLength , int sendDelay , int receiveDelay ) {
        //The lock keyword ensures that one thread does not enter a critical section of code while another thread is in the critical section.
        //If another thread tries to enter a locked code, it will wait, block, until the object is released.

        lock (packetLock){

            int sentDataLength=0;
            int expectedReceiveLength;
           // Dim x As System.Runtime.InteropServices.Marshal

            expectedReceiveLength = receiveLength;

            if ((myOutPipe != INVALID_HANDLE_VALUE) && (myInPipe != INVALID_HANDLE_VALUE)) {
               
               //UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'
                
                if (MPUSBWrite(myOutPipe, Marshal.UnsafeAddrOfPinnedArrayElement(sendData, 0).ToInt32(),  sendLength,ref sentDataLength, sendDelay) == MPUSB_SUCCESS) {

                   //UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'

                    if (MPUSBRead(myInPipe, Marshal.UnsafeAddrOfPinnedArrayElement(receiveData, 0).ToInt32(), expectedReceiveLength,ref receiveLength, receiveDelay) == MPUSB_SUCCESS) {

                        if (receiveLength == expectedReceiveLength) {
                          //  sendReceivePacket == 1; // Success!
                            return 1 ;
                        }
                       if (receiveLength < expectedReceiveLength) {
                           // SendReceivePacket = 2 // Partially failed, incorrect receive length
                            return 2;
                       }
                    }
                    else
                        CheckInvalidHandle();
                }
                else
                    CheckInvalidHandle();
                
            }

          //  SendReceivePacket = 0 ; // Operation Failed
            return 0;
    }
}

    private static void CheckInvalidHandle(){
        if (Marshal.GetLastWin32Error()== ERROR_INVALID_HANDLE) {

            // Most likely cause of the error is the board was disconnected.

            CloseMPUSBDevice();
        }
        else
            MessageBox.Show("Error Code : " +(Marshal.GetLastWin32Error()));
      
        }




    }
}







  


