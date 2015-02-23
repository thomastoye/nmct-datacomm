Imports System.Runtime.InteropServices.Marshal
Imports System.Runtime.InteropServices



Public Class MPUSB

    '======================================================================================
    '        VB.NET Example code MPUSBAPI.DLL
    '        Free to use code supplied by www.comvcon.com
    '        Use this code at you own will and risk
    '        Please do not remove this line: www.comvcon.com
    '======================================================================================

    'Functions for use in Visual Basic from MPUSBAPI.DLL
    'You can directly include this file in your application.

    <DllImport("mpusbapi.dll", EntryPoint:="_MPUSBGetDLLVersion", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Function MPUSBGetDLLVersion() As Integer

    End Function
    <DllImport("mpusbapi.dll", EntryPoint:="_MPUSBGetDeviceCount", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Function MPUSBGetDeviceCount(ByVal pVID_PID As String) As UInteger

    End Function
    <DllImport("mpusbapi.dll", EntryPoint:="_MPUSBOpen", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Function MPUSBOpen(ByVal instance As UInteger, ByVal pVID_PID As String, ByVal pEP As String, ByVal dwDir As UInteger, ByVal dwReserved As UInteger) As Integer

    End Function
    <DllImport("mpusbapi.dll", EntryPoint:="_MPUSBClose", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Function MPUSBClose(ByVal handle As Integer) As Integer

    End Function

    <DllImport("mpusbapi.dll", EntryPoint:="_MPUSBRead", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Function MPUSBRead(ByVal handle As Integer, ByVal pData As Integer, ByVal dwLen As Integer, ByRef pLength As Integer, ByVal dwMilliseconds As Integer) As Integer

    End Function

    <DllImport("mpusbapi.dll", EntryPoint:="_MPUSBWrite", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Function MPUSBWrite(ByVal handle As Integer, ByVal pData As Integer, ByVal dwLen As Integer, ByRef pLength As Integer, ByVal dwMilliseconds As Integer) As Integer

    End Function

    <DllImport("mpusbapi.dll", EntryPoint:="_MPUSBReadInt", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Function MPUSBReadInt(ByVal handle As Integer, ByRef pData() As Byte, ByVal dwLen As Integer, ByRef pLength As Integer, ByVal dwMilliseconds As Integer) As Integer

    End Function

    'Functions and Data required from WIN32 API
    Private Const INVALID_HANDLE_VALUE As Short = -1
    Private Const ERROR_INVALID_HANDLE As Short = 6

    Private Declare Function GetLastError Lib "kernel32" () As Integer

    'Constants for connecting to Microchip FS Demo board
    Private Const vid_pid As String = "vid_04d8&pid_000c"
    Private Const out_pipe As String = "\MCHP_EP1"
    Private Const in_pipe As String = "\MCHP_EP1"

    Private Const MPUSB_FAIL As Short = 0
    Private Const MPUSB_SUCCESS As Short = 1

    Private Const MP_WRITE As Short = 0
    Private Const MP_READ As Short = 1

    'constanten om de poorten te bepalen
    Private Const write_digital_out_port_D As Integer = &H10
    Private Const read_digital_in_port_B As Integer = &H12
    Private Const write_analog_out As Integer = &H14
    Private Const read_analog_in As Integer = &H15
    Private Const write_digital_out_port_E As Integer = &H16
    Private Const read_digital_out_port_D As Integer = &H18
    Private Const anal_buffer_out As Integer = &H1B
    'toevoegen om int16 in te lezen
    Private Shared read_digital_in_port_E As Integer = &H0

    'Declare the IN PIPE and OUT PIPE Public variables
    ' Private Shared myInPipe, myOutPipe As Integer
    Private Shared myInPipe As Integer = INVALID_HANDLE_VALUE
    Private Shared myOutPipe As Integer = INVALID_HANDLE_VALUE

    'synclock object
    Private Shared PacketLock As New Object

    'Following are the top-level Functionality to access the board

    Public Shared Sub Wait(ByVal lMS As Long)
        'deze routine wacht in milliseconden
        System.Threading.Thread.Sleep(lMS)
    End Sub

    Public Shared Function OpenMPUSBDevice() As Integer
        Dim tempPipe As Integer
        Dim count As Integer

        ' Always open one device only 
        tempPipe = INVALID_HANDLE_VALUE
        count = MPUSBGetDeviceCount(vid_pid)

        If count > 0 Then
            myOutPipe = MPUSBOpen(0, vid_pid, out_pipe, MP_WRITE, 0)
            myInPipe = MPUSBOpen(0, vid_pid, in_pipe, MP_READ, 0)

            If myOutPipe = INVALID_HANDLE_VALUE Or myInPipe = INVALID_HANDLE_VALUE Then
                MsgBox(Str(myOutPipe) & Str(myInPipe) & "Failed to open data pipes.")
                myOutPipe = myInPipe = INVALID_HANDLE_VALUE
                Return -1
            End If
            Return 0
        Else
            MsgBox("No devices connected.")
            Return -1
        End If

    End Function

    Public Shared Sub CloseMPUSBDevice()
        If myOutPipe <> INVALID_HANDLE_VALUE Then
            MPUSBClose(myOutPipe)
            myOutPipe = INVALID_HANDLE_VALUE
        End If
        If myInPipe <> INVALID_HANDLE_VALUE Then
            MPUSBClose(myInPipe)
            myInPipe = INVALID_HANDLE_VALUE
        End If
    End Sub

    Public Shared Function GetVersion() As String
        Dim send_buf(64) As Byte
        Dim receive_buf(64) As Byte
        Dim RecvLength As Integer

        If myOutPipe <> INVALID_HANDLE_VALUE And myInPipe <> INVALID_HANDLE_VALUE Then
            RecvLength = 4
            send_buf(0) = 0 '0x0 - READ_VERSION
            send_buf(1) = 2

            If (SendReceivePacket(send_buf, 2, receive_buf, RecvLength, 1000, 1000) = 1) Then

                If (RecvLength <> 4 Or receive_buf(0) <> 0) Then

                    Return "Failed to obtain version information."
                Else
                    Return "Demo Version  " & Str(receive_buf(3)) & "." & Str(receive_buf(2))

                End If
            Else
                Return "failed to obtain version information"
            End If
        Else
            Return "failed to obtain version information"
        End If
    End Function

    Public Shared Function ReadDigitalInPortB() As Byte
        Dim send_buf(64) As Byte
        Dim receive_buf(64) As Byte

        Dim RecvLength As Integer

        RecvLength = 2
        send_buf(0) = read_digital_in_port_B   'Comando
        If (SendReceivePacket(send_buf, 1, receive_buf, RecvLength, 1000, 1000) = 1) Then
            If (RecvLength = 2 And receive_buf(0) = read_digital_in_port_B) Then
                Return receive_buf(1)
            Else
                Throw New Exception("USB Operation Failed : digital in")
            End If
        Else
            Throw New Exception("USB Operation Failed : digital in")
        End If
    End Function

    Public Shared Function ReadDigitalOutPortD() As Int16
        Dim send_buf(64) As Byte
        Dim receive_buf(64) As Byte

        Dim RecvLength As Integer

        RecvLength = 2
        send_buf(0) = read_digital_out_port_D
        'Comando
        If (SendReceivePacket(send_buf, 1, receive_buf, RecvLength, 1000, 1000) = 1) Then
            If (RecvLength = 2 And receive_buf(0) = read_digital_out_port_D) Then
                Return (receive_buf(1) Or (read_digital_in_port_E << 8))
            Else
                Throw New Exception("USB Operation Failed : read digital out")
            End If
        Else
            Throw New Exception("USB Operation Failed : read digital out")
        End If
    End Function

    Public Shared Sub WriteDigitalOutPortD(ByVal data As Int16)
        Dim send_buf(64) As Byte
        Dim receive_buf(64) As Byte
        Dim RecvLength As Integer

        'poort E
        If myOutPipe <> INVALID_HANDLE_VALUE And myInPipe <> INVALID_HANDLE_VALUE Then
            RecvLength = 1
            send_buf(0) = write_digital_out_port_E  '&H16 
            send_buf(1) = (data >> 8)
            read_digital_in_port_E = send_buf(1)

            If (SendReceivePacket(send_buf, 3, receive_buf, RecvLength, 1000, 1000) = 1) Then

                If (RecvLength <> 1 Or receive_buf(0) <> write_digital_out_port_E) Then
                    Throw New Exception("Failed to update LED")
                End If
            End If
        End If
        'poort D
        If myOutPipe <> INVALID_HANDLE_VALUE And myInPipe <> INVALID_HANDLE_VALUE Then
            RecvLength = 1
            send_buf(0) = write_digital_out_port_D '&H10 
            send_buf(1) = (data And &HFF)

            If (SendReceivePacket(send_buf, 3, receive_buf, RecvLength, 1000, 1000) = 1) Then
                If (RecvLength <> 1 Or receive_buf(0) <> write_digital_out_port_D) Then
                    Throw New Exception("Failed to update LED")
                End If
            End If
        End If

    End Sub

    Public Shared Sub WriteAnalogOut(ByVal kanaal As Byte, ByVal data As Int16)
        Dim send_buf(64) As Byte
        Dim receive_buf(64) As Byte

        Dim RecvLength As Integer

        RecvLength = 1
        send_buf(0) = write_analog_out '	//Comando
        send_buf(1) = kanaal  '//kanaal
        send_buf(2) = CByte((data >> 8) And &H3)   '//Data --> 2 bits mayor 
        send_buf(3) = CByte(data And &HFF)    '//Dato --> 8 bits peso minor, total 10 bits (0-1023)

        If (SendReceivePacket(send_buf, 4, receive_buf, RecvLength, 1000, 1000) = 1) Then
            If ((RecvLength <> 1) Or (receive_buf(0) <> write_analog_out)) Then
                Throw New Exception("USB Operation Failed" & "Function analogout")
            End If
        Else
            Throw New Exception("USB Operation Failed" & "Function analogout")
        End If
    End Sub
    'buffer meegeven
    Public Shared Sub WriteAnalogOut(ByVal kanaal As Byte, ByVal data() As Byte)
        Dim send_buf(64) As Byte
        Dim receive_buf(64) As Byte
        If (data.Length < 64) Then
            Throw New ArgumentException("Size of the buffer must be larger than 64", "data")
        End If

        Dim RecvLength As Integer
        RecvLength = 1
        send_buf(0) = anal_buffer_out '	//Comando
        send_buf(1) = kanaal  '//kanaal

        'For index As Integer = 2 To 63
        '    send_buf(index) = CByte((data(index - 1) >> 8) And &H3)   '//Data --> 2 bits mayor 
        '    send_buf(index + 1) = CByte(data(index - 1) And &HFF)    '//Dato --> 8 bits peso minor, total 10 bits (0-1023)
        'Next

        Buffer.BlockCopy(data, 0, send_buf, 2, 62)

        If (SendReceivePacket(send_buf, 64, receive_buf, RecvLength, 1000, 1000) = 1) Then
            If ((RecvLength <> 1) Or (receive_buf(0) <> anal_buffer_out)) Then
                Throw New Exception("USB Operation Failed" & "Function analogout")
            End If
        Else
            Throw New Exception("USB Operation Failed" & "Function analogout")
        End If
    End Sub
    Public Shared Function ReadAnalogIn(ByVal kanaal As Byte) As Int16
        Dim iAout As Int16
        Dim send_buf(64) As Byte
        Dim receive_buf(64) As Byte

        Dim RecvLength As Integer = 4

        send_buf(0) = read_analog_in 'Comando
        send_buf(1) = CByte(kanaal) 'Dato
        If (SendReceivePacket(send_buf, 2, receive_buf, RecvLength, 1000, 1000) = 1) Then

            If ((RecvLength = 4) And (receive_buf(0) = read_analog_in)) Then
                iAout = receive_buf(2)
                iAout = iAout << 8
                iAout = iAout Or receive_buf(3)
                Return iAout
            Else
                Throw (New Exception("USB Operation Failed Analog in"))
            End If
        Else
            Throw New Exception("USB Operation Failed Analog in")
        End If
    End Function


    '/////////////////////////////////////////////////////////////////////////////
    '//
    '// A typical application would send a command to the target device and expect
    '// a response.
    '// SendReceivePacket is a wrapper function that facilitates the
    '// send command / read response paradigm
    '//
    '// SendData - pointer to data to be sent
    '// SendLength - length of data to be sent
    '// ReceiveData - Points to the buffer that receives the data read from the call
    '// ReceiveLength - Points to the number of bytes read
    '// SendDelay - time-out value for MPUSBWrite operation in milliseconds
    '// ReceiveDelay - time-out value for MPUSBRead operation in milliseconds
    '//

    Private Shared Function SendReceivePacket(ByRef SendData() As Byte, ByRef SendLength As Integer, ByRef ReceiveData() As Byte, ByRef ReceiveLength As Integer, ByVal SendDelay As Integer, ByVal ReceiveDelay As Integer) As Integer

        SyncLock packetlock


            Dim SentDataLength As Integer
            Dim ExpectedReceiveLength As Long
            ' Dim x As System.Runtime.InteropServices.Marshal

            ExpectedReceiveLength = ReceiveLength

            If (myOutPipe <> INVALID_HANDLE_VALUE And myInPipe <> INVALID_HANDLE_VALUE) Then

                'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'

                If (MPUSBWrite(myOutPipe, UnsafeAddrOfPinnedArrayElement(SendData, 0).ToInt32(), SendLength, SentDataLength, SendDelay) = MPUSB_SUCCESS) Then

                    'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'

                    If (MPUSBRead(myInPipe, UnsafeAddrOfPinnedArrayElement(ReceiveData, 0).ToInt32(), ExpectedReceiveLength, ReceiveLength, ReceiveDelay) = MPUSB_SUCCESS) Then

                        If (ReceiveLength = ExpectedReceiveLength) Then
                            SendReceivePacket = 1 '// Success!
                            Exit Function
                        ElseIf (ReceiveLength < ExpectedReceiveLength) Then
                            SendReceivePacket = 2 '// Partially failed, incorrect receive length
                            Exit Function
                        End If

                    Else
                        CheckInvalidHandle()
                    End If
                Else
                    CheckInvalidHandle()
                End If
            End If

            SendReceivePacket = 0 '// Operation Failed

        End SyncLock
    End Function

    Private Shared Sub CheckInvalidHandle()
        If (GetLastError() = ERROR_INVALID_HANDLE) Then

            '// Most likely cause of the error is the board was disconnected.

            CloseMPUSBDevice()
        Else
            MsgBox("Error Code : " & Str(GetLastError()))
        End If
    End Sub
End Class
