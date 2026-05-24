Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Module Program

    Private Const PORT As Integer = 5000

    Sub Main()
        Dim htmlPath As String = Path.Combine(AppContext.BaseDirectory, "index.html")
        If Not File.Exists(htmlPath) Then
            htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "index.html")
        End If
        Dim html As String = File.ReadAllText(htmlPath, Encoding.UTF8)

        Dim listener As New TcpListener(IPAddress.Any, PORT)
        listener.Start()

        Dim localIP As String = GetLocalNetworkIP()

        Console.OutputEncoding = Encoding.UTF8
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("==================================================")
        Console.WriteLine("  KALKULATORI VB.NET - Serveri po punon!")
        Console.WriteLine("==================================================")
        Console.ResetColor()
        Console.WriteLine()
        Console.Write("  Localhost : ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine("http://localhost:" & PORT)
        Console.ResetColor()
        Console.Write("  Network   : ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine("http://" & localIP & ":" & PORT)
        Console.ResetColor()
        Console.WriteLine()
        Console.WriteLine("  Shtyp Ctrl+C per ta ndaluar serverin.")
        Console.WriteLine("==================================================")
        Console.WriteLine()

        While True
            Dim client = listener.AcceptTcpClient()
            Dim t As New Thread(Sub() HandleClient(client, html))
            t.IsBackground = True
            t.Start()
        End While
    End Sub

    Private Function GetLocalNetworkIP() As String
        Try
            Using s As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)
                s.Connect("8.8.8.8", 65530)
                Dim ep = CType(s.LocalEndPoint, IPEndPoint)
                Return ep.Address.ToString()
            End Using
        Catch
            Try
                For Each ip In Dns.GetHostAddresses(Dns.GetHostName())
                    If ip.AddressFamily = AddressFamily.InterNetwork AndAlso
                       Not IPAddress.IsLoopback(ip) Then
                        Return ip.ToString()
                    End If
                Next
            Catch
            End Try
            Return "127.0.0.1"
        End Try
    End Function

    Private Sub HandleClient(client As TcpClient, html As String)
        Try
            client.ReceiveTimeout = 3000
            client.SendTimeout = 3000
            Using stream = client.GetStream()
                Dim buffer(8191) As Byte
                Dim total As New StringBuilder()
                Dim bytesRead As Integer = 0

                Do
                    bytesRead = stream.Read(buffer, 0, buffer.Length)
                    If bytesRead <= 0 Then Exit Do
                    total.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead))
                    If total.ToString().Contains(vbCrLf & vbCrLf) Then Exit Do
                Loop While stream.DataAvailable

                Dim request As String = total.ToString()
                Dim firstLine As String = request.Split(New String() {vbCrLf}, StringSplitOptions.None)(0)
                Dim parts = firstLine.Split(" "c)
                If parts.Length < 2 Then Return
                Dim method As String = parts(0)
                Dim path As String = parts(1)

                Dim responseBody As String = ""
                Dim contentType As String = "text/html; charset=utf-8"
                Dim status As String = "200 OK"

                If method = "POST" AndAlso path.StartsWith("/api/vleresoj") Then
                    Dim bodyStart As Integer = request.IndexOf(vbCrLf & vbCrLf)
                    Dim bodyText As String = ""
                    If bodyStart >= 0 Then
                        bodyText = request.Substring(bodyStart + 4).Trim()
                    End If
                    Dim piket As Double
                    If Double.TryParse(bodyText, Globalization.NumberStyles.Any,
                                       Globalization.CultureInfo.InvariantCulture, piket) Then
                        If piket >= 50 Then
                            responseBody = "PASS|Nxënësi kalon testin (" & piket & " pikë)"
                        Else
                            responseBody = "FAIL|Nxënësi mbetet (" & piket & " pikë)"
                        End If

                        Console.ForegroundColor = If(piket >= 50, ConsoleColor.Green, ConsoleColor.Red)
                        Console.WriteLine("  [VLERESIM] " & piket & " pike -> " &
                                          If(piket >= 50, "KALON", "MBETET"))
                        Console.ResetColor()
                    Else
                        responseBody = "WARN|Ju lutem shkruani një numër!"
                    End If
                    contentType = "text/plain; charset=utf-8"

                ElseIf method = "GET" AndAlso (path = "/" OrElse path = "/index.html") Then
                    responseBody = html

                Else
                    status = "404 Not Found"
                    responseBody = "Not Found"
                    contentType = "text/plain; charset=utf-8"
                End If

                Dim bodyBytes = Encoding.UTF8.GetBytes(responseBody)
                Dim header = "HTTP/1.1 " & status & vbCrLf &
                             "Content-Type: " & contentType & vbCrLf &
                             "Content-Length: " & bodyBytes.Length & vbCrLf &
                             "Access-Control-Allow-Origin: *" & vbCrLf &
                             "Cache-Control: no-store" & vbCrLf &
                             "Connection: close" & vbCrLf & vbCrLf
                Dim headerBytes = Encoding.UTF8.GetBytes(header)
                stream.Write(headerBytes, 0, headerBytes.Length)
                stream.Write(bodyBytes, 0, bodyBytes.Length)
                stream.Flush()
            End Using
        Catch
        Finally
            Try
                client.Close()
            Catch
            End Try
        End Try
    End Sub

End Module
