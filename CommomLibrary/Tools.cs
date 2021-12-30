﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compass.CommomLibrary
{
    public static class Tools
    {
        public static void SendMail(string body, string emails = "alex.marques@cpas.com.br; bruno.araujo@cpas.com.br", string subject = "Execução automática")
        {

            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            
            msg.IsBodyHtml = true;

            msg.BodyEncoding = System.Text.Encoding.UTF8;

            msg.Body = body;

            msg.Subject = subject;

            msg.Sender = msg.From = new System.Net.Mail.MailAddress("cpas.robot@gmail.com");

            msg.ReplyToList.Add(new System.Net.Mail.MailAddress("bruno.araujo@cpas.com.br"));

            // var emails = "douglas.canducci@cpas.com.br;pedro.modesto@cpas.com.br;diana.lima@cpas.com.br;natalia.biondo@cpas.com.br;bruno.araujo@cpas.com.br;alex.marques@cpas.com.br";

            foreach (var to in emails.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.To.Add(to);
            }


            System.Net.Mail.SmtpClient cli = new System.Net.Mail.SmtpClient();

            cli.Host = "smtp.gmail.com";
            cli.Port = 587;
            cli.Credentials = new System.Net.NetworkCredential("cpas.robot@gmail.com", "cp@s9876");

            cli.EnableSsl = true;

            cli.Send(msg);
        }

        public static string GetMonthNumAbrev(int month)
        {

            switch (month)
            {
                case 1: return "01_jan";
                case 2: return "02_fev";
                case 3: return "03_mar";
                case 4: return "04_abr";
                case 5: return "05_mai";
                case 6: return "06_jun";
                case 7: return "07_jul";
                case 8: return "08_ago";
                case 9: return "09_set";
                case 10: return "10_out";
                case 11: return "11_nov";
                case 12: return "12_dez";

                default:
                    return null;
            }
        }

        public static void moveDirectory(string fuente, string destino)
        {
            if (!System.IO.Directory.Exists(destino))
            {
                System.IO.Directory.CreateDirectory(destino);
            }
            String[] files = System.IO.Directory.GetFiles(fuente);
            String[] directories = System.IO.Directory.GetDirectories(fuente);
            foreach (string s in files)
            {
                var dest = System.IO.Path.Combine(destino, System.IO.Path.GetFileName(s));
                if (System.IO.File.Exists(dest)) System.IO.File.Delete(dest);
                System.IO.File.Move(s, dest);
            }
            foreach (string d in directories)
            {
                var d1 = System.IO.Path.Combine(fuente, System.IO.Path.GetFileName(d));
                moveDirectory(d1, System.IO.Path.Combine(destino, System.IO.Path.GetFileName(d)));
                System.IO.Directory.Delete(d1, true);
            }

        }

        public static DateTime[] inicioVR = new DateTime[] {
new DateTime(2017,10,15),
new DateTime(2018,11,4),
new DateTime(2019,10,20),
new DateTime(2020,10,18),
new DateTime(2021,10,17),
new DateTime(2022,10,16),
new DateTime(2023,10,15),
new DateTime(2024,10,20),
new DateTime(2025,10,19),
new DateTime(2026,10,18),
new DateTime(2027,10,17),
new DateTime(2028,10,15),
new DateTime(2029,10,21),
new DateTime(2030,10,20),
new DateTime(2031,10,19),
new DateTime(2032,10,17),

            };

        public static DateTime[] fimVR = new DateTime[] {
new DateTime(2018,02,17),
new DateTime(2019,02,16),
new DateTime(2020,02,15),
new DateTime(2021,02,20),
new DateTime(2022,02,19),
new DateTime(2023,02,18),
new DateTime(2024,02,17),
new DateTime(2025,02,15),
new DateTime(2026,02,14),
new DateTime(2027,02,20),
new DateTime(2028,02,19),
new DateTime(2029,02,17),
new DateTime(2030,02,16),
new DateTime(2031,02,15),
new DateTime(2032,02,14),
            };

        public static DateTime[] feriados = new DateTime[] {
                new DateTime(2017,01,01),
new DateTime(2017,02,28),
new DateTime(2017,04,14),
new DateTime(2017,04,16),
new DateTime(2017,04,21),
new DateTime(2017,05,01),
new DateTime(2017,06,15),
new DateTime(2017,09,07),
new DateTime(2017,10,12),
new DateTime(2017,11,02),
new DateTime(2017,11,15),
new DateTime(2017,12,25),
new DateTime(2018,01,01),
new DateTime(2018,02,13),
new DateTime(2018,03,30),
new DateTime(2018,04,01),
new DateTime(2018,04,21),
new DateTime(2018,05,01),
new DateTime(2018,05,31),
new DateTime(2018,09,07),
new DateTime(2018,10,12),
new DateTime(2018,11,02),
new DateTime(2018,11,15),
new DateTime(2018,12,25),
new DateTime(2019,01,01),
new DateTime(2019,03,05),
new DateTime(2019,04,19),
new DateTime(2019,05,01),
new DateTime(2019,06,20),
new DateTime(2019,09,07),
new DateTime(2019,10,12),
new DateTime(2019,11,02),
new DateTime(2019,11,15),
new DateTime(2019,12,25),
new DateTime(2020,01,01),
new DateTime(2020,02,25),
new DateTime(2020,04,10),
new DateTime(2020,04,12),
new DateTime(2020,04,21),
new DateTime(2020,05,01),
new DateTime(2020,06,11),
new DateTime(2020,09,07),
new DateTime(2020,10,12),
new DateTime(2020,11,02),
new DateTime(2020,11,15),
new DateTime(2020,12,25),
new DateTime(2021,01,01),
new DateTime(2021,02,16),
new DateTime(2021,04,02),
new DateTime(2021,04,04),
new DateTime(2021,04,21),
new DateTime(2021,05,01),
new DateTime(2021,06,03),
new DateTime(2021,09,07),
new DateTime(2021,10,12),
new DateTime(2021,11,02),
new DateTime(2021,11,15),
new DateTime(2021,12,25),
new DateTime(2022,01,01),
new DateTime(2022,03,01),
new DateTime(2022,04,15),
new DateTime(2022,04,17),
new DateTime(2022,04,21),
new DateTime(2022,05,01),
new DateTime(2022,06,16),
new DateTime(2022,09,07),
new DateTime(2022,10,12),
new DateTime(2022,11,02),
new DateTime(2022,11,15),
new DateTime(2022,12,25),
new DateTime(2023,01,01),
new DateTime(2023,02,21),
new DateTime(2023,04,07),
new DateTime(2023,04,09),
new DateTime(2023,04,21),
new DateTime(2023,05,01),
new DateTime(2023,06,08),
new DateTime(2023,09,07),
new DateTime(2023,10,12),
new DateTime(2023,11,02),
new DateTime(2023,11,15),
new DateTime(2023,12,25),
new DateTime(2024,01,01),
new DateTime(2024,02,13),
new DateTime(2024,03,29),
new DateTime(2024,03,31),
new DateTime(2024,04,21),
new DateTime(2024,05,01),
new DateTime(2024,05,30),
new DateTime(2024,09,07),
new DateTime(2024,10,12),
new DateTime(2024,11,02),
new DateTime(2024,11,15),
new DateTime(2024,12,25),
new DateTime(2025,01,01),
new DateTime(2025,03,04),
new DateTime(2025,04,18),
new DateTime(2025,04,20),
new DateTime(2025,04,21),
new DateTime(2025,05,01),
new DateTime(2025,06,19),
new DateTime(2025,09,07),
new DateTime(2025,10,12),
new DateTime(2025,11,02),
new DateTime(2025,11,15),
new DateTime(2025,12,25),
new DateTime(2026,01,01),
new DateTime(2026,02,17),
new DateTime(2026,04,03),
new DateTime(2026,04,05),
new DateTime(2026,04,21),
new DateTime(2026,05,01),
new DateTime(2026,06,04),
new DateTime(2026,09,07),
new DateTime(2026,10,12),
new DateTime(2026,11,02),
new DateTime(2026,11,15),
new DateTime(2026,12,25),
new DateTime(2027,01,01),
new DateTime(2027,02,09),
new DateTime(2027,03,26),
new DateTime(2027,03,28),
new DateTime(2027,04,21),
new DateTime(2027,05,01),
new DateTime(2027,05,27),
new DateTime(2027,09,07),
new DateTime(2027,10,12),
new DateTime(2027,11,02),
new DateTime(2027,11,15),
new DateTime(2027,12,25),
new DateTime(2028,01,01),
new DateTime(2028,02,29),
new DateTime(2028,04,14),
new DateTime(2028,04,16),
new DateTime(2028,04,21),
new DateTime(2028,05,01),
new DateTime(2028,06,15),
new DateTime(2028,09,07),
new DateTime(2028,10,12),
new DateTime(2028,11,02),
new DateTime(2028,11,15),
new DateTime(2028,12,25),
new DateTime(2029,01,01),
new DateTime(2029,02,13),
new DateTime(2029,03,30),
new DateTime(2029,04,01),
new DateTime(2029,04,21),
new DateTime(2029,05,01),
new DateTime(2029,05,31),
new DateTime(2029,09,07),
new DateTime(2029,10,12),
new DateTime(2029,11,02),
new DateTime(2029,11,15),
new DateTime(2029,12,25),
new DateTime(2030,01,01),
new DateTime(2030,03,05),
new DateTime(2030,04,19),
new DateTime(2030,05,01),
new DateTime(2030,06,20),
new DateTime(2030,09,07),
new DateTime(2030,10,12),
new DateTime(2030,11,02),
new DateTime(2030,11,15),
new DateTime(2030,12,25),
new DateTime(2031,01,01),
new DateTime(2031,02,25),
new DateTime(2031,04,11),
new DateTime(2031,04,13),
new DateTime(2031,04,21),
new DateTime(2031,05,01),
new DateTime(2031,06,12),
new DateTime(2031,09,07),
new DateTime(2031,10,12),
new DateTime(2031,11,02),
new DateTime(2031,11,15),
new DateTime(2031,12,25),
new DateTime(2032,01,01),
new DateTime(2032,02,10),
new DateTime(2032,03,26),
new DateTime(2032,03,28),
new DateTime(2032,04,21),
new DateTime(2032,05,01),
new DateTime(2032,05,27),
new DateTime(2032,09,07),
new DateTime(2032,10,12),
new DateTime(2032,11,02),
new DateTime(2032,11,15),
new DateTime(2032,12,25),
new DateTime(2033,01,01),
new DateTime(2033,03,01),
new DateTime(2033,04,15),
new DateTime(2033,04,17),
new DateTime(2033,04,21),
new DateTime(2033,05,01),
new DateTime(2033,06,16),
new DateTime(2033,09,07),
new DateTime(2033,10,12),
new DateTime(2033,11,02),
new DateTime(2033,11,15),
new DateTime(2033,12,25),

            };

        public static Tuple<int, int, int> GetHorasPatamares(DateTime ini, DateTime fim, bool patamares2019)
        {
            Tuple<int, int, int>[,] horasPatamares;

            // var horasPatamares = new Tuple<int, int, int>[] {
            //                new Tuple<int,int,int>(3,14,7),
            //                new Tuple<int,int,int>(0,5,19),
            if (patamares2019)
                horasPatamares = new Tuple<int, int, int>[,] {
                { new Tuple<int,int,int>(08,08,08), new Tuple<int,int,int>(00,03,21) },
                { new Tuple<int,int,int>(08,08,08), new Tuple<int,int,int>(00,03,21) },
                { new Tuple<int,int,int>(08,08,08), new Tuple<int,int,int>(00,03,21) },
                { new Tuple<int,int,int>(10,06,08), new Tuple<int,int,int>(00,04,20) },
                { new Tuple<int,int,int>(12,05,07), new Tuple<int,int,int>(00,04,20) },
                { new Tuple<int,int,int>(12,05,07), new Tuple<int,int,int>(00,04,20) },
                { new Tuple<int,int,int>(12,05,07), new Tuple<int,int,int>(00,04,20) },
                { new Tuple<int,int,int>(12,05,07), new Tuple<int,int,int>(00,04,20) },
                { new Tuple<int,int,int>(10,06,08), new Tuple<int,int,int>(00,04,20) },
                { new Tuple<int,int,int>(10,06,08), new Tuple<int,int,int>(00,04,20) },
                { new Tuple<int,int,int>(08,08,08), new Tuple<int,int,int>(00,03,21) },
                { new Tuple<int,int,int>(08,08,08), new Tuple<int,int,int>(00,03,21) }
            };
            else
                horasPatamares = new Tuple<int, int, int>[,] {
                           { new Tuple<int,int,int>(3,14,7), new Tuple<int,int,int>(0,5,19) }
                };




            var p1 = 0;
            var p2 = 0;
            var p3 = 0;

            for (DateTime dt = ini; dt <= fim; dt = dt.AddDays(1))
            {
                Tuple<int, int, int> pat;


                if (patamares2019)
                    pat = (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday || feriados.Contains(dt)) ?
                       horasPatamares[dt.Month - 1, 1] :
                       horasPatamares[dt.Month - 1, 0];

                else
                    pat = (dt.DayOfWeek == DayOfWeek.Sunday || feriados.Contains(dt)) ?
                           horasPatamares[0, 1] :
                           horasPatamares[0, 0];


                p1 += pat.Item1;
                p2 += pat.Item2;
                p3 += pat.Item3;

                if (inicioVR.Contains(dt))
                {
                    p3--;
                }
                else if (fimVR.Contains(dt))
                {
                    if (patamares2019) p3++;
                    else p2++;

                }

            }

            return new Tuple<int, int, int>(p1, p2, p3);
        }

        public static Dictionary<int, string> GetIntervalosHoararios(DateTime data)
        {
            var feriados = Tools.feriados;
            Boolean ehFeriado = false;

            if (feriados.Any(x => x.Date == data.Date))
            {
                ehFeriado = true;
            }

            Dictionary<int, string> NOVaMARutil = new Dictionary<int, string>() {//<hora,patamar>
                    {0, "LEVE"},
                    {1, "LEVE"},
                    {2, "LEVE"},
                    {3, "LEVE"},
                    {4, "LEVE"},
                    {5, "LEVE"},
                    {6, "LEVE"},
                    {7, "LEVE"},
                    {8, "MEDIA"},
                    {9, "MEDIA"},
                    {10, "PESADA"},
                    {11, "PESADA"},
                    {12, "PESADA"},
                    {13, "PESADA"},
                    {14, "PESADA"},
                    {15, "PESADA"},
                    {16, "PESADA"},
                    {17, "PESADA"},
                    {18, "MEDIA"},
                    {19, "MEDIA"},
                    {20, "MEDIA"},
                    {21, "MEDIA"},
                    {22, "MEDIA"},
                    {23, "MEDIA"},

                };

            Dictionary<int, string> NOVaMARfer = new Dictionary<int, string>() {//<hora,patamar>
                    {0, "LEVE"},
                    {1, "LEVE"},
                    {2, "LEVE"},
                    {3, "LEVE"},
                    {4, "LEVE"},
                    {5, "LEVE"},
                    {6, "LEVE"},
                    {7, "LEVE"},
                    {8, "LEVE"},
                    {9, "LEVE"},
                    {10, "LEVE"},
                    {11, "LEVE"},
                    {12, "LEVE"},
                    {13, "LEVE"},
                    {14, "LEVE"},
                    {15, "LEVE"},
                    {16, "LEVE"},
                    {17, "LEVE"},
                    {18, "LEVE"},
                    {19, "LEVE"},
                    {20, "MEDIA"},
                    {21, "MEDIA"},
                    {22, "MEDIA"},
                    {23, "LEVE"},

                };

            Dictionary<int, string> MAIOaAGOutil = new Dictionary<int, string>() {//<hora,patamar>
                    {0, "LEVE"},
                    {1, "LEVE"},
                    {2, "LEVE"},
                    {3, "LEVE"},
                    {4, "LEVE"},
                    {5, "LEVE"},
                    {6, "LEVE"},
                    {7, "MEDIA"},
                    {8, "MEDIA"},
                    {9, "MEDIA"},
                    {10, "PESADA"},
                    {11, "PESADA"},
                    {12, "PESADA"},
                    {13, "PESADA"},
                    {14, "PESADA"},
                    {15, "PESADA"},
                    {16, "PESADA"},
                    {17, "PESADA"},
                    {18, "PESADA"},
                    {19, "PESADA"},
                    {20, "PESADA"},
                    {21, "PESADA"},
                    {22, "MEDIA"},
                    {23, "MEDIA"},

                };

            Dictionary<int, string> MAIOaAGOfer = new Dictionary<int, string>() {//<hora,patamar>
                    {0, "LEVE"},
                    {1, "LEVE"},
                    {2, "LEVE"},
                    {3, "LEVE"},
                    {4, "LEVE"},
                    {5, "LEVE"},
                    {6, "LEVE"},
                    {7, "LEVE"},
                    {8, "LEVE"},
                    {9, "LEVE"},
                    {10, "LEVE"},
                    {11, "LEVE"},
                    {12, "LEVE"},
                    {13, "LEVE"},
                    {14, "LEVE"},
                    {15, "LEVE"},
                    {16, "LEVE"},
                    {17, "LEVE"},
                    {18, "MEDIA"},
                    {19, "MEDIA"},
                    {20, "MEDIA"},
                    {21, "MEDIA"},
                    {22, "LEVE"},
                    {23, "LEVE"},

                };

            Dictionary<int, string> ABRSETOUTutil = new Dictionary<int, string>() {//<hora,patamar>
                    {0, "LEVE"},
                    {1, "LEVE"},
                    {2, "LEVE"},
                    {3, "LEVE"},
                    {4, "LEVE"},
                    {5, "LEVE"},
                    {6, "LEVE"},
                    {7, "LEVE"},
                    {8, "MEDIA"},
                    {9, "MEDIA"},
                    {10, "PESADA"},
                    {11, "PESADA"},
                    {12, "PESADA"},
                    {13, "PESADA"},
                    {14, "PESADA"},
                    {15, "PESADA"},
                    {16, "PESADA"},
                    {17, "PESADA"},
                    {18, "PESADA"},
                    {19, "PESADA"},
                    {20, "MEDIA"},
                    {21, "MEDIA"},
                    {22, "MEDIA"},
                    {23, "MEDIA"},

                };

            Dictionary<int, string> ABRSETOUTfer = new Dictionary<int, string>() {//<hora,patamar>
                    {0, "LEVE"},
                    {1, "LEVE"},
                    {2, "LEVE"},
                    {3, "LEVE"},
                    {4, "LEVE"},
                    {5, "LEVE"},
                    {6, "LEVE"},
                    {7, "LEVE"},
                    {8, "LEVE"},
                    {9, "LEVE"},
                    {10, "LEVE"},
                    {11, "LEVE"},
                    {12, "LEVE"},
                    {13, "LEVE"},
                    {14, "LEVE"},
                    {15, "LEVE"},
                    {16, "LEVE"},
                    {17, "LEVE"},
                    {18, "MEDIA"},
                    {19, "MEDIA"},
                    {20, "MEDIA"},
                    {21, "MEDIA"},
                    {22, "LEVE"},
                    {23, "LEVE"},

                };

            switch(data.Month)
            {
                case 1:
                case 2:
                case 3:
                case 11:
                case 12:
                    if (ehFeriado || data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return NOVaMARfer;
                    }
                    else
                    {
                        return NOVaMARutil;
                    }
                case 4:
                case 9:
                case 10:
                    if (ehFeriado || data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return ABRSETOUTfer;
                    }
                    else
                    {
                        return ABRSETOUTutil;
                    }
                case 5:
                case 6:
                case 7:
                case 8:
                    if (ehFeriado || data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return MAIOaAGOfer;
                    }
                    else
                    {
                        return MAIOaAGOutil;
                    }

                    
            }
            return null;
        }

        public static Tuple<int, int, int> GetWeekPatamares(DateTime date, bool patamares2019)
        {
            var inicioSemanaOperativa = date;
            while (inicioSemanaOperativa.DayOfWeek != DayOfWeek.Saturday) inicioSemanaOperativa = inicioSemanaOperativa.AddDays(-1);
            var fimSemanaOperativa = inicioSemanaOperativa.AddDays(6);

            return GetHorasPatamares(inicioSemanaOperativa, fimSemanaOperativa, patamares2019);
        }

        public static int[] GetCalendarDaysFromOperativeMonth(int year, int month)
        {

            var checkSum = 0;

            var result = new int[6];
            int i = 0;

            var monthStart = new DateTime(year, month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            int daysToRemove = 0;
            if (monthStart.DayOfWeek != DayOfWeek.Saturday) daysToRemove = 1 + (int)monthStart.DayOfWeek;

            var week = monthStart.AddDays(-daysToRemove);//.AddDays(-7);

            var weekStart = monthStart;
            var weekEnd = week.AddDays(7);

            do
            {
                result[i++] = (weekEnd - weekStart).Days;
                checkSum += (weekEnd - weekStart).Days;

                weekStart = weekEnd;


                if (weekStart.AddDays(7) > nextMonthStart)
                    weekEnd = nextMonthStart;
                else
                    weekEnd = weekStart.AddDays(7);

            } while (weekStart.Month == month);

            if ((nextMonthStart - monthStart).Days != checkSum) throw new Exception();


            return result;
        }
        public static List<Tuple<DateTime,int>>GetNumDatSem(DateTime data, int num)
        {
            List<Tuple<DateTime, int>> semanas = new List<Tuple<DateTime, int>>();
            for (int i = 0; i <= 11; i++)//horizonte de doze semanas
            {
                var NumSem = GetWeekNumberAndYear(data);
                semanas.Add(new Tuple<DateTime, int>(data, NumSem.Item1));
                data = data.AddDays(7);
            }
            return semanas;
        }
        public static Tuple<int, int> GetWeekNumberAndYear(DateTime date)
        {

            var nextFriday = date.DayOfWeek == DayOfWeek.Saturday ? date.AddDays(6) :
                date.AddDays((int)DayOfWeek.Friday - (int)date.DayOfWeek);

            var y = nextFriday.Year;

            var yearStart = new DateTime(y, 1, 1);
            yearStart = yearStart.AddDays(-1 * ((int)yearStart.DayOfWeek + 1) % 7);

            var weekNumber = (int)Math.Floor(((date - yearStart).TotalDays) / 7) + 1;

            return new Tuple<int, int>(weekNumber, y);
        }

        public static List<Acomph> GetAcomphData(DateTime inicio, DateTime fim)
        {
            using (IPDOEntities ctx = new IPDOEntities())
            {
                return ctx.ACOMPH.Where(x => x.Data >= inicio && x.Data <= fim)
                    .Select(x => new Acomph() { dt = x.Data, posto = x.Posto, qNat = x.Vaz_nat, qInc = x.Vaz_Inc })
                    .ToList();
            }
        }
        public static (DateTime revDate, int rev) GetCurrRev(DateTime date)
        {
            var currRevDate = date;

            do
            {
                currRevDate = currRevDate.AddDays(1);
            } while (currRevDate.DayOfWeek != DayOfWeek.Friday);
            var currRevNum = currRevDate.Day / 7 - (currRevDate.Day % 7 == 0 ? 1 : 0);

            return (currRevDate, currRevNum);
        }
    }

    public class SemanaOperativa
    {
        public int HorasPat1 { get; set; }
        public int HorasPat2 { get; set; }
        public int HorasPat3 { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }

        //public SemanaOperativa(DateTime inicio)
        //{
        //    this.Inicio = Inicio;
        //    this.Fim = Inicio.AddDays(6);
        //    var pat = Tools.GetWeekPatamares(Inicio);
        //    this.HorasPat1 = pat.Item1;
        //    this.HorasPat2 = pat.Item2;
        //    this.HorasPat3 = pat.Item3;
        //}

        public SemanaOperativa(DateTime i, DateTime f, bool patamares2019)
        {
            this.Inicio = i;
            this.Fim = f;
            var pat = Tools.GetHorasPatamares(i, f, patamares2019);
            this.HorasPat1 = pat.Item1;
            this.HorasPat2 = pat.Item2;
            this.HorasPat3 = pat.Item3;

        }

        public SemanaOperativa(DateTime f, bool patamares2019 = true) : this(f.AddDays(-6), f, patamares2019)
        {

        }


        public SemanaOperativa Proxima()
        {
            return new SemanaOperativa(Fim.AddDays(1), Fim.AddDays(7), true);
        }

        public int NumeroDaSemana
        {
            get
            {
                var mop = MesOperativo.CreateSemanal(Fim.Year, 1, true);
                var num = ((int)(Fim - mop.SemanasOperativas.First().Inicio).TotalDays / 7 + 1);
                return num;
            }
        }

        public int Ano { get { return Fim.Year; } }
    }



    public class MesOperativo
    {
        private MesOperativo()
        {
            this.SemanasOperativas = new List<SemanaOperativa>();
        }
        public static MesOperativo CreateSemanal(int ano, int mes, bool patamares2019)
        {
            var mOper = new MesOperativo();
            mOper.Ano = ano;
            mOper.Mes = mes;

            var datetime = new DateTime(ano, mes, 1);
            while (datetime.DayOfWeek != DayOfWeek.Saturday) datetime = datetime.AddDays(-1);
            mOper.Inicio = datetime;

            datetime = datetime.AddDays(6);
            while (datetime.Month == mes)
            {
                mOper.SemanasOperativas.Add(
                    new SemanaOperativa(datetime.AddDays(-6), datetime, patamares2019)
                    );

                datetime = datetime.AddDays(7);
            }

            if (datetime.Day == 7)
            {
                mOper.SemanasOperativas.Add(
                    new SemanaOperativa((new DateTime(ano, mes, 1)).AddMonths(1), (new DateTime(ano, mes, 1)).AddMonths(2).AddDays(-1), patamares2019)
                    );

                mOper.Fim = datetime.AddDays(-7);
                mOper.DiasMes2 = 0;
            }
            else
            {
                mOper.SemanasOperativas.Add(
                    new SemanaOperativa(datetime.AddDays(-6), datetime, patamares2019)
                    );

                mOper.SemanasOperativas.Add(
                    new SemanaOperativa(datetime.AddDays(1), (new DateTime(ano, mes, 1)).AddMonths(2).AddDays(-1), patamares2019)
                    );

                mOper.Fim = datetime.AddDays(-7);
                mOper.DiasMes2 = datetime.Day;

            }
            mOper.MesSeguinte = datetime.Month;
            mOper.AnoSeguinte = datetime.Year;

            mOper.Estagios = mOper.SemanasOperativas.Count - 1;

            for (int i = 0; i < mOper.SemanasOperativas.Count; i++)
            {
                if (Tools.inicioVR.Any(x => mOper.SemanasOperativas[i].Inicio <= x && mOper.SemanasOperativas[i].Fim >= x)) mOper.EstagioInicioHorarioVerao = i + 1;
                if (Tools.fimVR.Any(x => mOper.SemanasOperativas[i].Inicio <= x && mOper.SemanasOperativas[i].Fim >= x)) mOper.EstagioFimHorarioVerao = i + 1;
            }


            return mOper;
        }

        public static MesOperativo CreateSemanalDadgnl(int ano, int mes, bool patamares2019)
        {
            var mOper = new MesOperativo();
            mOper.Ano = ano;
            mOper.Mes = mes;

            var datetime = new DateTime(ano, mes, 1);
            while (datetime.DayOfWeek != DayOfWeek.Saturday) datetime = datetime.AddDays(-1);
            mOper.Inicio = datetime;

            datetime = datetime.AddDays(6);
            while (datetime.Month == mes)
            {
                mOper.SemanasOperativas.Add(
                    new SemanaOperativa(datetime.AddDays(-6), datetime, patamares2019)
                    );

                datetime = datetime.AddDays(7);
            }

            //if (datetime.Day == 7)
            //{
            //    mOper.SemanasOperativas.Add(
            //        new SemanaOperativa((new DateTime(ano, mes, 1)).AddMonths(1), datetime, patamares2019)
            //        );

            //    mOper.Fim = datetime.AddDays(-7);
            //    mOper.DiasMes2 = 0;
            //}
            //else
            //{
                //mOper.SemanasOperativas.Add(
                //    new SemanaOperativa(datetime.AddDays(-6), datetime, patamares2019)
                //    );

                mOper.SemanasOperativas.Add(
                    new SemanaOperativa(datetime.AddDays(1), (new DateTime(ano, mes, 1)).AddMonths(2).AddDays(-1), patamares2019)
                    );

                mOper.Fim = datetime.AddDays(-7);
                mOper.DiasMes2 = datetime.Day;

            //}
            mOper.MesSeguinte = datetime.Month;
            mOper.AnoSeguinte = datetime.Year;

            mOper.Estagios = mOper.SemanasOperativas.Count - 1;

            for (int i = 0; i < mOper.SemanasOperativas.Count; i++)
            {
                if (Tools.inicioVR.Any(x => mOper.SemanasOperativas[i].Inicio <= x && mOper.SemanasOperativas[i].Fim >= x)) mOper.EstagioInicioHorarioVerao = i + 1;
                if (Tools.fimVR.Any(x => mOper.SemanasOperativas[i].Inicio <= x && mOper.SemanasOperativas[i].Fim >= x)) mOper.EstagioFimHorarioVerao = i + 1;
            }


            return mOper;
        }

        public static MesOperativo CreateMensal(int ano, int mes, bool patamares2019)
        {
            var mOper = new MesOperativo();

            mOper.Ano = ano;
            mOper.Mes = mes;

            var datetime = new DateTime(ano, mes, 1);
            //while (datetime.DayOfWeek != DayOfWeek.Saturday) datetime = datetime.AddDays(-1);
            mOper.Inicio = datetime;
            mOper.Fim = datetime.AddMonths(1).AddDays(-1);

            mOper.SemanasOperativas.Add(
                new SemanaOperativa(mOper.Inicio, mOper.Fim, patamares2019)
                );
            mOper.SemanasOperativas.Add(
                new SemanaOperativa(mOper.Inicio.AddMonths(1), datetime.AddMonths(2).AddDays(-1), patamares2019)
                );

            mOper.DiasMes2 = 0;

            mOper.MesSeguinte = datetime.AddMonths(1).Month;
            mOper.AnoSeguinte = datetime.AddMonths(1).Year;

            mOper.Estagios = 1;

            for (int i = 0; i < mOper.SemanasOperativas.Count; i++)
            {
                if (Tools.inicioVR.Any(x => mOper.SemanasOperativas[i].Inicio <= x && mOper.SemanasOperativas[i].Fim >= x)) mOper.EstagioInicioHorarioVerao = i + 1;
                if (Tools.fimVR.Any(x => mOper.SemanasOperativas[i].Inicio <= x && mOper.SemanasOperativas[i].Fim >= x)) mOper.EstagioFimHorarioVerao = i + 1;
            }

            return mOper;

        }

        public List<SemanaOperativa> SemanasOperativas { get; set; }

        public int Mes { get; private set; }

        public int Ano { get; private set; }

        public int Estagios { get; private set; }

        public int EstagiosReaisDoMesAtual { get {
                return Estagios - (DiasMes2 > 0 ? 1 : 0);
            } }

        public int DiasMes2 { get; private set; }

        public DateTime Inicio { get; private set; }
        public DateTime Fim { get; private set; }

        public int? EstagioInicioHorarioVerao { get; private set; }

        public int? EstagioFimHorarioVerao { get; private set; }
        public int MesSeguinte { get; set; }
        public int AnoSeguinte { get; set; }
    }


    public class Acomph
    {

        public DateTime dt { get; set; }
        public int posto { get; set; }
        public double qInc { get; set; }
        public double qNat { get; set; }

        public int semana
        {
            get
            {
                return Compass.CommomLibrary.Tools.GetWeekNumberAndYear(dt).Item1;
            }
        }
        public int ano
        {
            get
            {
                return Compass.CommomLibrary.Tools.GetWeekNumberAndYear(dt).Item2;
            }
        }
    }



}
