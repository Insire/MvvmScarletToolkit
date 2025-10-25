```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
AMD Ryzen 7 9800X3D, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                                    | Mean     | Error    | StdDev   | Ratio | RatioSD |
|------------------------------------------ |---------:|---------:|---------:|------:|--------:|
| MediatR_Send_Request_With_Response        | 55.46 ns | 1.119 ns | 1.332 ns |  1.00 |    0.03 |
| MediatR_Send_Request                      | 54.27 ns | 0.786 ns | 0.735 ns |  0.98 |    0.03 |
| SimpleMediator_Send_Request_With_Response | 28.39 ns | 0.270 ns | 0.252 ns |  0.51 |    0.01 |
| SimpleMediator_Send_Request               | 24.29 ns | 0.224 ns | 0.209 ns |  0.44 |    0.01 |
