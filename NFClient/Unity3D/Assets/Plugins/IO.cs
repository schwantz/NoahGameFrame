using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.IO.Ports;
using System;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class IO : MonoBehaviour
{
	[DllImport ("native")]
	private static extern float add (float x, float y);

	[DllImport ("native")]
	private static extern int tty_open (int port, int mbaudrate, int parity);

	[DllImport ("native")]
	private static extern void tty_close (int tty_fd);

	[DllImport ("native")]
	private static extern int tty_writemessage (int tty_fd, byte[] msg, int len);

	[DllImport ("native")]
	private static extern int tty_readmessage (int tty_fd, byte[] msg, int len);

	[DllImport ("native")]
	private static extern int tty_readcode (int tty_fd, byte[] msg);

	int file_fd, file_fdgpio, buf_wr, buf_rd, buf_wrgpio, buf_rdgpio, a, a0, a00, coin,preXcode;
	byte[] xmessage, usb16message, ledmessage, xcode, rx_buf, xcodegpio, rx_bufgpio;
	byte tt;
	Int32 out0, out1, out2, out3, out4, out5, out6, out7;
	Int32 out8, out9, out10, out11, out12, out13, out14, out15;
	Int32 out16, out17, out18, out19, out20, out21, out22, out23;
	Int32 out24, out25, out26, out27, out28, out29, out30, out31;
	Int32 out32, out33, out34, out35, out36, out37, out38, out39;
	Thread thread4, thread2,thread3;
	float playtimes,cointimes;
	bool f, b, r, l, play, cantplay, now0, checkplaytime,cancoin,readycoin;
	public Text showtime,showread,showreadend;
	WaitForSeconds w01;
	// Use this for initialization
    public int count1=0;
    private UnityEngine.Object thisLock = new UnityEngine.Object();
    static Semaphore mySemaphore = new Semaphore(1, 1);
    static Semaphore mySemaphore_setPlayTimes = new Semaphore(1, 1);
    int delayGap=10;

    void setPlayTimes(float aTime, float direct =0)
    {
        mySemaphore_setPlayTimes.WaitOne();

        if (direct == 0)
        {
            playtimes = aTime;
        }
        else
        {
            playtimes += direct;
        }
        mySemaphore_setPlayTimes.Release();

    }
    void Start ()
    {
        preXcode = 0;
           thisLock = new UnityEngine.Object();
        count1 = 10000;
		file_fd = tty_open (26, 115200, 0);
		file_fdgpio = tty_open (2, 115200, 0);  //gpio
		xmessage = new byte[8];
		usb16message = new byte[6];
		ledmessage = new byte[8];
		xcode = new byte[1];
		rx_buf = new byte[1024];
		xcodegpio = new byte[1];
		rx_bufgpio = new byte[1024];
		w01 = new WaitForSeconds (0.001f);
		now0 = true;
		readycoin = true;
		out0 = 0;
		out1 = 0;
		out2 = 0;
		out3 = 0;
		out4 = 0;
		out5 = 0;
		out6 = 0;
		out7 = 0;
		out8 = 0;
		out9 = 0;
		out10 = 1; //投幣口1開啟
		out11 = 1; //投幣口2開啟
		out12 = 0;
		out13 = 0;
		out14 = 0;
		out15 = 0;
		out16 = 0;
		out17 = 0;
		out18 = 0;
		out19 = 0;
		out20 = 0;
		out21 = 0;
		out22 = 0;
		out23 = 0;
		out24 = 0;
		out25 = 0;
		out26 = 0;
		out27 = 0;
		out28 = 0;
		out29 = 0;
		out30 = 0;
		out31 = 0;
		out32 = 0;
		out33 = 0;
		out34 = 0;
		out35 = 0;
		out36 = 0;
		out37 = 0;
		out38 = 0;
		out39 = 0;
		Outio ();
		buf_wr = 0;
		buf_rd = 0;
		buf_wrgpio = 0;
		buf_rdgpio = 0;
		playtimes = 30f;
        //showtime.text = playtimes.ToString ();
        thread4 = new Thread(readio);
        thread4.IsBackground = true;
        thread4.Start();
        thread2 = new Thread (chk_io);
		thread2.IsBackground = true;
		thread2.Start ();
		thread3 = new Thread (readgpio);
		thread3.IsBackground = true;
		thread3.Start ();
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
        usb16message[0] = 0x23;
        usb16message[1] = 0x41;
        usb16message[2] = 0x5c;
        usb16message[3] = 0x72;
        usb16message[4] = 0x5c;
        usb16message[5] = 0x6e;
        f = false;
        b = false;
        r = false;
        l = false;

    }
    private static int pi = 0;
    public string GettestValue()
    {
        string sstr="";
        if (pi == 0)
        {
            sstr = "file_fd=" + file_fd.ToString();
        }
        else if (pi == 1)
        {
            sstr = "file_fdgpio=" + file_fdgpio.ToString();
        }
        else if (pi == 2)
        {
            sstr = "cantplay=" + cantplay.ToString();
        }
        else if (pi == 3)
        {
            sstr = "rx_buf[buf_rd]=" + rx_buf[buf_rd].ToString();

        }
        else if (pi == 4) 
        {
            int tmp_point = buf_rd - 1;
            if (tmp_point < 0)
                tmp_point = 1023;
            sstr = "rx_buf [tmp_point]=" + rx_buf[tmp_point].ToString();

        }
        else if (pi == 5)
        {
            sstr = "buf_rd=" + buf_rd.ToString();

        }
        else if (pi == 6)
        {
            sstr = "buf_wr=" + buf_wr.ToString();

        }
        pi++;
        if (pi > 6)
            pi = 0;
        return sstr;
    }

    // Update is called once per frame
    private void OnGUI()
    {
        string ss;
        ss=GUI.TextField(new Rect(10, 10, 150, 50), delayGap.ToString());
        delayGap = Convert.ToInt32(ss);

    }
    void Update ()
	{
        //count1++;
        //if (buf_wr != buf_rd)
        //{
        //    if (rx_buf[buf_rd] == 0x53)
        //    {
        //        int tmp_point = buf_rd - 1;
        //        if (tmp_point < 0)
        //            tmp_point = 1023;
        //        if (rx_buf[tmp_point] == 0x40)
        //        {
        //            StartCoroutine("end");

        //        }
        //    }
        //    buf_rd++;
        //    if (buf_rd >= 1024)
        //        buf_rd = 0;
        //}
        //else
        //{

        //}

        if (!readycoin) {
			cointimes += Time.deltaTime;
			if (cointimes > 0.3f) {
				readycoin = true;
				cointimes = 0f;
			}
		}
		if (cantplay) {
			return;
		}
		if (playtimes <= 0f) {
			play = false;
            setPlayTimes(0);
			//playtimes = 0f;
			Playtimes (0);
			//showtime.text = "0";
			f = false;
			b = false;
			r = false;
			l = false;
			grab ();
		}
		if (play) {
			//playtimes -= Time.deltaTime;
            setPlayTimes(0,-1 * Time.deltaTime);
            //showtime.text = ((int)playtimes).ToString ();

        }
    }

    void chk_io () //檢查是否讀到值
	{
        int tick=0;
		while (true) {
			if (1 == tty_readcode (file_fd, xcode)) {           //usb
                lock (thisLock)
                {
                    int v = xcode[0];
                    if (v== 0x53)
                    {
                        if (preXcode == 0x40)
                        {
                            Debug.Log("StartCoroutine('ioend')");
                            //StartCoroutine("ioend");
                            
                            tick = Environment.TickCount;
                        }
                    }
                    preXcode = v;
                    //Debug.Log("buf_wr="+ buf_wr+" xcode[0]=" + xcode[0]);
                    //rx_buf[buf_wr] = xcode[0];
                    //buf_wr++;
                    //if (buf_wr >= 1024)
                    //    buf_wr = 0;
                }

            }
            if (1 == tty_readcode (file_fdgpio, xcodegpio)) {	//gpio
				rx_bufgpio [buf_wrgpio] = xcodegpio [0];
				buf_wrgpio++;
				if (buf_wrgpio >= 1024)
					buf_wrgpio = 0;
			}
            if (tick!=0 && (Environment.TickCount- tick > 6000))
            {
                cantplay = false;
                setPlayTimes(30);
                Playtimes(30);
                tick = 0;
            }
        }        
	}
	void readio ()
	{ //檢析usb值
        int tick =0;
        while (true)
        {
            if (f || b || l || r)
            {
                tty_writemessage(file_fd, usb16message, delayGap);
                Thread.Sleep(2);


            }
            if (play && Environment.TickCount - tick > 300)
            {
                Playtimes((int)playtimes);
                tick = Environment.TickCount;
            }
        }
    }
    void readgpio ()
	{ //檢析gpio值
		int tmp_point;

		while (true) {

 
            if (buf_wrgpio != buf_rdgpio) {
				if (rx_bufgpio [buf_rdgpio] == 0x0A) {
					tmp_point = buf_rdgpio - 1;
					if (tmp_point < 0)
						tmp_point = 1023;

					if (rx_bufgpio [tmp_point] == 0x0D) {
						tmp_point = buf_rdgpio - 7;
						if (tmp_point < 0) {
							tmp_point = 1024 - (7 - buf_rdgpio);
						}
						if (rx_bufgpio [tmp_point] == 0x40) {
							if((rx_bufgpio [tmp_point+3]& 0x80) == 0x80){ //數投幣
								if (readycoin) {
									readycoin = false;
									if (!cancoin) {
										cancoin = true;
										coin += 1;
										howmuchcoin (coin);
                                        //showread.text = rx_bufgpio[tmp_point + 1].ToString() + rx_bufgpio[tmp_point + 2].ToString() + rx_bufgpio[tmp_point + 3].ToString()
                                        //    + rx_bufgpio[tmp_point + 4].ToString() + rx_bufgpio[tmp_point + 5].ToString() + rx_bufgpio[tmp_point + 6].ToString()
                                        //    + rx_bufgpio[tmp_point + 7].ToString();
                                    }
								}
							}else{
								cancoin = false;
								//showreadend.text = rx_bufgpio [tmp_point + 1].ToString()+ rx_bufgpio [tmp_point + 2].ToString()+ rx_bufgpio [tmp_point + 3].ToString()
								//	+ rx_bufgpio [tmp_point + 4].ToString()+ rx_bufgpio [tmp_point + 5].ToString()+ rx_bufgpio [tmp_point + 6].ToString()
								//	+rx_bufgpio [tmp_point + 7].ToString();
							}
						}
					}
				}
				buf_rdgpio++;
				if (buf_rdgpio >= 1024)
					buf_rdgpio = 0;
			} else {

			}
				
		}

	}

	public void onoff ()  //測試gpio全on/off
	{
		if (now0) {
			now0 = false;
			out0 = 1;
			out1 = 1;
			out2 = 1;
			out3 = 1;
			out4 = 1;
			out5 = 1;
			out6 = 1;
			out7 = 1;
			out8 = 1;
			out9 = 1;
			out10 = 1;
			out11 = 1;
			out12 = 1;
			out13 = 1;
			out14 = 1;
			out15 = 1;
			out16 = 1;
			out17 = 1;
			out18 = 1;
			out19 = 1;
			out20 = 1;
			out21 = 1;
			out22 = 1;
			out23 = 1;
			out24 = 1;
			out25 = 1;
			out26 = 1;
			out27 = 1;
			out28 = 1;
			out29 = 1;
			out30 = 1;
			out31 = 1;
			out32 = 1;
			out33 = 1;
			out34 = 1;
			out35 = 1;
			out36 = 1;
			out37 = 1;
			out38 = 1;
			out39 = 1;
		} else {
			now0 = true;
			out0 = 0;
			out1 = 0;
			out2 = 0;
			out3 = 0;
			out4 = 0;
			out5 = 0;
			out6 = 0;
			out7 = 0;
			out8 = 0;
			out9 = 0;
			out10 = 0;
			out11 = 0;
			out12 = 0;
			out13 = 0;
			out14 = 0;
			out15 = 0;
			out16 = 0;
			out17 = 0;
			out18 = 0;
			out19 = 0;
			out20 = 0;
			out21 = 0;
			out22 = 0;
			out23 = 0;
			out24 = 0;
			out25 = 0;
			out26 = 0;
			out27 = 0;
			out28 = 0;
			out29 = 0;
			out30 = 0;
			out31 = 0;
			out32 = 0;
			out33 = 0;
			out34 = 0;
			out35 = 0;
			out36 = 0;
			out37 = 0;
			out38 = 0;
			out39 = 0;
		}
		Outio ();
	}

	public void Outio ()
	{ //gpio out函數
		xmessage [0] = 0x23;
		xmessage [1] = 0x00;
		//xmessage[1] = BitConverter.GetBytes(out0*1+out1*2+out2*4+out3*8+out4*16+out5*32+out6*64+out7*128)[0];
		xmessage [2] = BitConverter.GetBytes (out24 * 1 + out25 * 2 + out26 * 4 + out27 * 8 + out28 * 16 + out29 * 32 + out30 * 64 + out31 * 128) [0];
		xmessage [3] = BitConverter.GetBytes (out16 * 1 + out17 * 2 + out18 * 4 + out19 * 8 + out20 * 16 + out21 * 32 + out22 * 64 + out23 * 128) [0];
		xmessage [4] = BitConverter.GetBytes (out8 * 1 + out9 * 2 + out10 * 4 + out11 * 8 + out12 * 16 + out13 * 32 + out14 * 64 + out15 * 128) [0];
		xmessage [5] = BitConverter.GetBytes (out0 * 1 + out1 * 2 + out2 * 4 + out3 * 8 + out4 * 16 + out5 * 32 + out6 * 64 + out7 * 128) [0];
		xmessage [6] = 0x0D;
		xmessage [7] = 0x0A;
		tty_writemessage (file_fdgpio, xmessage, 8);
	}

	public void writefram ()
	{ //fram寫入函數   1~4是寫入位置  5是寫入值
		xmessage [0] = 0x2a;
		xmessage [1] = 0x30;
		xmessage [2] = 0x00;
		xmessage [3] = 0x00;
		xmessage [4] = 0x00;
		xmessage [5] = 0x00;
		xmessage [6] = 0x0D;
		xmessage [7] = 0x0A;
		tty_writemessage (file_fd, xmessage, 8);
	}

	public void returnfram ()
	{ //呼叫fram傳值出來
		xmessage [0] = 0x2b;
		xmessage [1] = 0x30;
		xmessage [2] = 0x00;
		xmessage [3] = 0x00;
		xmessage [4] = 0x00;
		xmessage [5] = 0x00;
		xmessage [6] = 0x0D;
		xmessage [7] = 0x0A;
		tty_writemessage (file_fd, xmessage, 8);
	}

	public void grab () //啟動抓子
	{
		if (!cantplay) {
			if (!play) {
				play = true;
				checkplaytime = true;
				StartCoroutine ("checktimes");
			}
			cantplay = true;
			checkplaytime = false;
			f = false;
			b = false;
			r = false;
			l = false;
			//playtimes = 0f;
            setPlayTimes(0);
			Playtimes (0);
			//showtime.text = "0";
			usb16message [0] = 0x23;
			usb16message [1] = 0x45;
			usb16message [2] = 0x5c;
			usb16message [3] = 0x72;
			usb16message [4] = 0x5c;
			usb16message [5] = 0x6e;
			tty_writemessage (file_fd, usb16message, 6);
			if (play) {
				play = false;
			}
		}
	}

	public void fd ()  //按著前
	{
        if (!cantplay)
        {
            f = false;
            usb16message[1] = 0x41;
            f = true;
            if (!play)
            {
                play = true;
                checkplaytime = true;
            }
        }
    }

	public void fp ()　//放開前
	{
		f = false;
	}

	public void bd ()　//按著後
	{
        if (!cantplay)
        {
            b = false;
            usb16message[1] = 0x42;
            b = true;
            if (!play)
            {
                play = true;
                checkplaytime = true;
            }
        }
    }

	public void bp ()　//放開後
	{
		b = false;
	}

	public void rd ()　//按著右
	{
		if (!cantplay) {
            r = false;
            usb16message[1] = 0x43;
            r = true;
            if (!play)
            {
                play = true;
                checkplaytime = true;
            }
        }
    }

	public void rp ()　//放開右
	{
		r = false;
	}

	public void ld ()　//按著左
	{
		if (!cantplay) {
            l = false;
            usb16message[1] = 0x44;
            l = true;
            if (!play)
            {
                play = true;
                checkplaytime = true;
            }
        }
    }

	public void lp ()　//放開左
	{
		l = false;
	}

	public void checkled ()	//ledmessage [i]  i值:對應位置顯示面板上1 2 3 顯示面板下4 5
	{
		if (a < 9) {
			a += 1;
		} else {
			a = 0;
		}
		for (int i = 0; i < 6; i++) {
			ledmessage [i]	= whatbyte (a.ToString ());
		}
		outled ();
	}

	void Playtimes (int a)
	{
        mySemaphore.WaitOne();
        a00 = a / 10;
		ledmessage [4]	= whatbyte (a00.ToString ());
		a0 = a % 10;
		ledmessage [5] = whatbyte (a0.ToString ());
		outled ();
        mySemaphore.Release();
    }

    byte whatbyte (string a)	//顯示面板字的函數
	{
		byte rebyte;
		rebyte = 0x00;
		switch (a) {
		case "0":
		case "D":
			rebyte = 0x40;
			break;	
		case "1":
			rebyte = 0x79;
			break;	
		case "2":
			rebyte = 0x24;
			break;	
		case "3":
			rebyte = 0x30;
			break;	
		case "4":
			rebyte = 0x19;
			break;	
		case "5":
			rebyte = 0x12;
			break;	
		case "6":
			rebyte = 0x02;
			break;	
		case "7":
			rebyte = 0x58;
			break;	
		case "8":
		case "B":
			rebyte = 0x00;
			break;	
		case "9":
			rebyte = 0x10;
			break;	
		case "A":
			rebyte = 0x08;
			break;
		case "C":
			rebyte = 0x46;
			break;
		case "E":
			rebyte = 0x06;
			break;
		}
		return rebyte;
	}

	void outled ()	//顯示面板輸出
	{
		out21 = 1;
		Outio ();
		for (int i = 7; i > 0; i--) {
			tt = ledmessage [i - 1];
			for (int j = 0; j < 8; j++) {
				if ((tt & 0x80) == 0x80) {
					out19 = 1;
					Outio ();
				} else {
					out19 = 0;
					Outio ();
				}
				out20 = 0;
				Outio ();
				out20 = 1;
				Outio ();
				tt <<= 1;

			}
		}
		out21 = 0;
		Outio ();
	}

	void howmuchcoin(int a){
		ledmessage [1]	= whatbyte ((a/100).ToString());
		ledmessage [2]	= whatbyte ((a%100/10).ToString());
		ledmessage [3]	= whatbyte ((a%10).ToString());
		outled ();
	}

	IEnumerator foward ()
	{
		while (f) {
			if (!play) {
				play = true;
				checkplaytime = true;
				StartCoroutine ("checktimes");
			}
			usb16message [0] = 0x23;
			usb16message [1] = 0x41;
			usb16message [2] = 0x5c;
			usb16message [3] = 0x72;
			usb16message [4] = 0x5c;
			usb16message [5] = 0x6e;
			tty_writemessage (file_fd, usb16message, 6);
			yield return w01;
		}
	}

	IEnumerator back ()
	{
		while (b) {
			if (!play) {
				play = true;
				checkplaytime = true;
				StartCoroutine ("checktimes");
			}
			usb16message [0] = 0x23;
			usb16message [1] = 0x42;
			usb16message [2] = 0x5c;
			usb16message [3] = 0x72;
			usb16message [4] = 0x5c;
			usb16message [5] = 0x6e;
			tty_writemessage (file_fd, usb16message, 6);
			yield return w01;
		}
	}

	IEnumerator right ()
	{
		while (r) {
			if (!play) {
				play = true;
				checkplaytime = true;
				//StartCoroutine ("checktimes");
			}
			usb16message [0] = 0x23;
			usb16message [1] = 0x43;
			usb16message [2] = 0x5c;
			usb16message [3] = 0x72;
			usb16message [4] = 0x5c;
			usb16message [5] = 0x6e;
			tty_writemessage (file_fd, usb16message, 6);
            //yield return w01;
            yield return null;
        }
    }

	IEnumerator left ()
	{
		while (l) {
			if (!play) {
				play = true;
				checkplaytime = true;
				//StartCoroutine ("checktimes");
			}
			usb16message [0] = 0x23;
			usb16message [1] = 0x44;
			usb16message [2] = 0x5c;
			usb16message [3] = 0x72;
			usb16message [4] = 0x5c;
			usb16message [5] = 0x6e;
			tty_writemessage (file_fd, usb16message, 6);
            yield return new WaitForSeconds(0.5f); ;
            //yield return null;
        }
    }

	IEnumerator ioend ()
	{
		yield return new WaitForSeconds (6f);
		cantplay = false;
		//playtimes = 30f;
        setPlayTimes(30);
        Playtimes(30);
    }

	IEnumerator checktimes ()
	{
		while (checkplaytime) {
			Playtimes ((int)playtimes);
			yield return new WaitForSeconds (0.3f);
		}
	}
}
