using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DxLibDLL;
using PShader;

/*
 * Form1のRendererが描画と処理を行うが、詳細についてはすべてscのupdate();が行っている。背景のみBGのみDraw();が行っている。
 * それで、scがMenuのときでもupdate();は特に何もしていなくて、sbscのupdateを呼び出して処理を行っている。
 * 
 * 
 * 
 * 
 * 
 */

namespace launcher2014
{
    public partial class Form1 : Form
    {
        static public Dictionary<string,int> PadState;
        Scene sc;
        int timer;
        string CurrentDirectory;
        BackGround BG;
        int xyst;
        int gantsu;
        int BGM_flag;
        bool isplaybgm;
        static public int bgm;
        int Bbutton;
        public Form1()
        {
            InitializeComponent();
            PadState = new Dictionary<string,int>();
            ClientSize = new Size(1280, 720);
            DX.SetUserWindow(this.Handle);
            if (DX.DxLib_Init() == -1)
            {
                return;
            }
            CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);
            sc = new Menu();
            BG = new BackGround("data/BackSky_.png");
            GetInput();
            Init();
            xyst = DX.LoadSoundMem("data/xyst.ogg");
            gantsu = DX.LoadSoundMem("data/ランチャー.wav");
            BGM_flag = 1;
            Bbutton = 0;
            bgm = xyst;
            isplaybgm = true;
        }
        public void Init()
        {
            timer = 0;
            PadState["DOWN"]    = 0;
            PadState["UP"]      = 0;
            PadState["LEFT"]    = 0;
            PadState["RIGHT"]   = 0;
            PadState["A"]       = 0;
            PadState["B"]       = 0;
        }
        public void Renderer()
        {
            DX.ClearDrawScreen();
            GetInput();
            BG.Draw();
            sc.update();
            if(sc.flag_calling_next)
            {
                sc = sc.next();
            }
            DX.ScreenFlip();
            
            if(DX.CheckHitKey(DX.KEY_INPUT_B)!=0)
            {
                Bbutton++;
            }
            else
            {
                Bbutton = 0;
            }
            if(Bbutton==1)
            {
                DX.StopSoundMem(bgm);
            if(BGM_flag ==0)
            {
                bgm = xyst;
                BGM_flag = 1;
            }
            else if(BGM_flag ==1)
            {
                bgm = gantsu;
                BGM_flag = 0;
            }
                isplaybgm = true;
            }
            if (isplaybgm)
            {
                
                DX.PlaySoundMem(bgm, DX.DX_PLAYTYPE_LOOP);
                isplaybgm = false;
            }
            timer++;
        }
        public void GetInput()
        {
            //Joypadの入力受け取り
            if((DX.GetJoypadInputState(DX.DX_INPUT_PAD1)&DX.PAD_INPUT_DOWN)!=0||DX.CheckHitKey(DX.KEY_INPUT_DOWN)!=0)
            {
                PadState["DOWN"]++;
            }
            else 
            {
                PadState["DOWN"] = 0;
            }
            if ((DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_UP) != 0||DX.CheckHitKey(DX.KEY_INPUT_UP)!=0)
            {
                PadState["UP"]++;
            }
            else 
            {             
                PadState["UP"] = 0;
            }
            if ((DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_LEFT) != 0||DX.CheckHitKey(DX.KEY_INPUT_LEFT)!=0)
            {
                PadState["LEFT"]++;
            }
            else
            {
                PadState["LEFT"] = 0;
            }
            if ((DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_RIGHT) != 0||DX.CheckHitKey(DX.KEY_INPUT_RIGHT)!=0)
            {
                PadState["RIGHT"]++;
            }
            else 
            {
                PadState["RIGHT"] = 0;
            }
            if ((DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_1) != 0||DX.CheckHitKey(DX.KEY_INPUT_Z)!=0)
            {
                PadState["A"]++;
            }
            else
            {
                PadState["A"] = 0;
            }
            if ((DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_2) != 0||DX.CheckHitKey(DX.KEY_INPUT_X)!=0)
            {
                PadState["B"]++;
            }
            else 
            {
                PadState["B"] = 0;
            }

        }
    }
    public class Scene
    {
        public bool flag_calling_next;
        public virtual void update(){ return; }
        public virtual Scene next() { return null; }
    };
    public class Menu:Scene
    {
        int timer;
        public SubScene sbsc;
        public Menu()
        {
            timer = 0;
            base.flag_calling_next = false;
            sbsc = new Start();
        }
        public override void update( )
        {
            sbsc.update();
            if (sbsc.flag_call_next)
            {
                sbsc = sbsc.next();
            }
            timer++;
        }
        public override Scene next()
        {
            return new Select();
        }
    }

    //使わなかった
    public class Select:Scene
    {
        int timer;
        List<string> paths;
        public Select()
        {
            timer = 0;
            base.flag_calling_next = false;
            paths = new List<string>();

        }
        public override void update()
        {
            if ((DX.GetJoypadInputState(DX.DX_INPUT_KEY_PAD1) & DX.PAD_INPUT_2)==1)
            {
                base.flag_calling_next = true;
            }
            else if((DX.CheckHitKey(DX.KEY_INPUT_X)==1))
            {
                base.flag_calling_next = true;
            }
            DX.DrawString(timer/10, 0, "Select", 0xffffff);
            timer++;
            return;
        }
        public override Scene next()
        {
            return new Menu() ;
        }
    }
    public class Start:SubScene
    {
        int timer;
        int GHandle;
        int logo;
        int title;
        bool flag_start_timer;
        public Start()
        {
            timer = 0;
            flag_start_timer = false;
            base.flag_call_next = false;
            GHandle = DX.LoadGraph("data/pressanykey.png");
            logo = DX.LoadGraph("data/ccslogo_big.png");
            title = DX.LoadGraph("data/title.png");
        }
        public override void update()
        {
            if (DX.GetJoypadInputState(DX.DX_INPUT_KEY_PAD1) != 0||DX.CheckHitKeyAll()!=0)
            {
               flag_start_timer = true;
            }
            if (flag_start_timer)
            {
                timer++;
            }

            if (timer == 30)
            {
                base.flag_call_next = true;
            }

            DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255 - timer * 255 / 30);
            DX.DrawRotaGraph(640, 250, 0.5, 0.0, logo, DX.TRUE);
            DX.DrawRotaGraph(640, 500, 1.0, 0.0, title, DX.TRUE);
            DX.DrawRotaGraph(640, 600, 1.0, 0, GHandle, DX.TRUE);
            DX.SetDrawBlendMode(DX.DX_BLENDGRAPHTYPE_ALPHA, 255);

            return;
        }
        public override SubScene next()
        {
            DX.DeleteGraph(GHandle);
            DX.DeleteGraph(logo);
            DX.DeleteGraph(title);
            return new GameSelect();
        }
    }
    public class GameSelect:SubScene
    {
        int timer;
        int timer_of_unavailable_button;
        bool flag_timer_start;
        int back;
        List<Game> game;
        public GameSelect()
        {
            //GetFilesでなんとかこうにかゲームのパスを取得する
            back = DX.LoadGraph("data/readme_back.png");
            game = new List<Game>();
            timer = 0;
            flag_timer_start = false;
            base.flag_call_next = false;
            string[] temp;
            temp = System.IO.Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory()+"/softwares/");
            foreach(var hoge in  temp)
            {
                game.Add(new Game(hoge));
            }
            for(int i = 0;i<game.Count;i++)
            {
                game[i].SetPos(i, game.Count);
            }
            timer_of_unavailable_button = 30;
            
        }
        public override void update()
        {
            //一番前に来てるゲームの選択
            var the_front_game = game.OrderBy(l => l.GetPosX()).Last();

            //キー入力とかの処理
            if (timer_of_unavailable_button == 0)
            {
                if ((DX.GetJoypadInputState(DX.DX_INPUT_KEY_PAD1) & DX.PAD_INPUT_1) != 0 || (DX.CheckHitKey(DX.KEY_INPUT_Z) != 0))
                {
                    the_front_game.run();
                    timer_of_unavailable_button = 30;
                }

                if ((DX.GetJoypadInputState(DX.DX_INPUT_KEY_PAD1) & DX.PAD_INPUT_2) != 0)
                {
                    flag_timer_start = true;
                }
                else if ((DX.CheckHitKey(DX.KEY_INPUT_X) == 1))
                {
                    flag_timer_start = true;
                }
            if ((DX.CheckHitKey(DX.KEY_INPUT_UP) != 0 || (DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_UP) !=0))
            {
                foreach(var hoge in game)
                {
                    hoge.RotaUpward();
                }
            }
            else if ((DX.CheckHitKey(DX.KEY_INPUT_DOWN) != 0|| (DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_DOWN) !=0))
            {

                foreach(var hoge in game)
                {
                    hoge.RotaDownward();
                }
            }
            }
            else
            {
                timer_of_unavailable_button--;
            }

            if(flag_timer_start)
            {
                timer++;
            }
            if(timer==30)
            {
                base.flag_call_next = true;
            }
            foreach(var hoge in game)
            {
                hoge.update();
            }

            
            //以下描画部分
            DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255 - timer * 255 / 30);
            DX.DrawGraph(770, 0, back, DX.TRUE);
             foreach(var hoge in game)
             {
                 hoge.Draw();
             }
             the_front_game.Draw();
             the_front_game.DrawReadme();
            DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 255);
            
            return;
        }
        public override SubScene next()
        {
            foreach(var hoge in game)
            {
                hoge.DeleteMember();
            }
            game.Clear();
            return new Start();
        }
    }
    public class SubScene
    {
        public bool flag_call_next;
        virtual public void update() { return; }
        virtual public SubScene next() { return null; }
    }
    public class Game
    {
        int ScreenShot;
        string path;
        DX.VECTOR Pos;
        float r;
        float theta;
        string readme;
        System.IO.StreamReader sr;
        int readme_y;
        int readme_y_max;
        public Game(string _game_path)
        { 
            path = _game_path;
            try
            {
                ScreenShot = DX.LoadGraph(System.IO.Directory.GetFiles(_game_path, "*.PNG").First().ToString());
            }
            catch
            {
                //ScreenShot = DX.LoadGraph(System.IO.Directory.GetFiles(_game_path, "*.png").ToString());
            }
            theta = 0.0f;
            r = 100.0f;
            Pos = new DX.VECTOR();
            LoadReadme();
            readme_y = 0;
        }
        public void SetPos(int i,int totalnum)
        {
            
            theta = (float)i / totalnum;
            Pos = DX.VGet(r * (float)Math.Cos(TransToRad( theta)) -200.0f, r * (float)Math.Sin(TransToRad( theta)), 20.0f);
        }
        public void Draw()
        {
            DX.DrawBillboard3D(Pos, 0.5f, 0.5f, 100.0f, 0.0f, ScreenShot, DX.TRUE);
        }
        public void update()
        {
            Pos = DX.VGet(r * (float)Math.Cos(TransToRad(theta)) - 170.0f, r * (float)Math.Sin(TransToRad(theta)), 20.0f);
        }
        public void RotaUpward()
        {
            theta += 0.005f;
        }
        public void RotaDownward()
        {
            theta -= 0.005f;
        }
        public double TransToRad(double _val)
        {
            return _val*2.0*Math.PI;
        }
        public void run()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
           
            p.StartInfo.WorkingDirectory = path;
            p.StartInfo.FileName = path +"/"+ System.IO.Path.GetFileName(path).ToString() + ".exe";
            DX.StopSoundMem(Form1.bgm);
            p.Start();
            p.WaitForExit();

            DX.PlaySoundMem(Form1.bgm, DX.DX_PLAYTYPE_LOOP);
            
        }
        public void LoadReadme()
        {
            try
            {
                sr = new System.IO.StreamReader(path + "/readme.txt",System.Text.Encoding.UTF8);
               
                while(sr.Peek()>=0)
                {
                    string temp = sr.ReadLine();
                    readme += temp + System.Environment.NewLine;
                    temp = string.Empty;
                }
                sr.Close();
            }
            catch
            {
                readme = string.Empty;
            }
        }
        public void DrawReadme()
        {
            
            int index = 0;
            int i = 0;
            
            if ((DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_LEFT) != 0 || DX.CheckHitKey(DX.KEY_INPUT_LEFT) != 0)
            {
                readme_y-=7;
            }
            if ((DX.GetJoypadInputState(DX.DX_INPUT_PAD1) & DX.PAD_INPUT_RIGHT) != 0 || DX.CheckHitKey(DX.KEY_INPUT_RIGHT) != 0)
            {
                readme_y+=7;
            }
            if(readme_y<0)
            {
                readme_y = 0;
            }
            else if(readme_y>readme_y_max)
            {
                readme_y = readme_y_max;
            }
            try
            {
                while (true)
                {
                    string buf = readme.Substring(index, 27);
                    int hoge = buf.IndexOf("\n");
                    if(hoge == -1)
                    {
                       DXP.DrawFormatString(800,40+i*20-readme_y,0xffffff,"{0}",buf);
                       index += 26;
                    }
                    else
                    {
                        DXP.DrawFormatString(800, 40+i*20-readme_y, 0xffffff, "{0}", buf.Substring(0,hoge));
                        index += hoge;
                    }
                    index++;
                    i++;
                   
                }
            }
            catch { }
            readme_y_max = i * 20;
        }
        public DX.VECTOR GetPos()
        {
            return Pos;
        }
        public void DeleteMember()
        {
            DX.DeleteGraph(ScreenShot);
        }
        public float GetPosX()
        {
            return Pos.x;
        }
    }
    public class BackGround
    {
        int Ghandle;
        int timer;
        DX.VERTEX3D[] vert;
        int myscreen;
        DX.VECTOR CameraPos;
        DX.VECTOR Target;
        public BackGround() 
        {
            Ghandle = 0;
            timer = 0;
            InitVertex();
        }
        public BackGround(string _ghandle)
        {
            Ghandle = DX.LoadGraph(_ghandle);
            timer = 0;
            vert = new DX.VERTEX3D[6];
            InitVertex();
            myscreen = DX.MakeScreen(1280, 720);
            CameraPos = new DX.VECTOR();
            InitCamPos(40.0f,0.0f,-200.0f);
            InitTarPos(0.0f, 0.0f, 0.0f);
        }
        private void InitVertex()
        {
       
            vert[0].pos = DX.VGet(-300.0f,180.0f,0.0f);
            vert[0].norm = DX.VGet(0.0f, 0.0f, -1.0f);
            vert[0].dif = DX.GetColorU8(255, 255, 255, 255);
            vert[0].spc = DX.GetColorU8(0, 0, 0, 0);
            vert[0].u = 0.0f;
            vert[0].v = 0.0f;
            vert[0].su = 0.0f;
            
            vert[1].pos = DX.VGet(320.0f, 180.0f, 0.0f);
            vert[1].norm = DX.VGet(0.0f, 0.0f, -1.0f);
            vert[1].dif = DX.GetColorU8(255, 255, 255, 255);
            vert[1].spc = DX.GetColorU8(0, 0, 0, 0);
            vert[1].u = 1.0f;
            vert[1].v = 0.0f;
           

            vert[2].pos = DX.VGet(-300.0f, -180.0f, 0.0f);
            vert[2].norm = DX.VGet(0.0f, 0.0f, -1.0f);
            vert[2].dif = DX.GetColorU8(255, 255, 255, 255);
            vert[2].spc = DX.GetColorU8(0, 0, 0, 0);
            vert[2].u = 0.0f;
            vert[2].v = 1.0f;
           
            vert[3].pos = DX.VGet(320.0f, 180.0f, 0.0f);
            vert[3].norm = DX.VGet(0.0f, 0.0f, -1.0f);
            vert[3].dif = DX.GetColorU8(255, 255, 255, 255);
            vert[3].spc = DX.GetColorU8(0, 0, 0, 0);
            vert[3].u = 1.0f;
            vert[3].v = 0.0f;
           

            vert[4].pos = DX.VGet(-300.0f, -180.0f, 0.0f);
            vert[4].norm = DX.VGet(0.0f, 0.0f, -1.0f);
            vert[4].dif = DX.GetColorU8(255, 255, 255, 255);
            vert[4].spc = DX.GetColorU8(0, 0, 0, 0);
            vert[4].u = 0.0f;
            vert[4].v = 1.0f;
           
            
            vert[5].pos = DX.VGet(320.0f, -180.0f, 0.0f);
            vert[5].norm = DX.VGet(0.0f, 0.0f, -1.0f);
            vert[5].dif = DX.GetColorU8(255, 255, 255, 255);
            vert[5].spc = DX.GetColorU8(0, 0, 0, 0);
            vert[5].u = 1.0f;
            vert[5].v = 1.0f;
            for (int i = 0; i < vert.Length; i++)
            {
                vert[i].su = vert[i].sv = 0.0f;
            }
        

        }
        private void InitCamPos(float _x,float _y,float _z)
        {
            CameraPos.x = _x;
            CameraPos.y = _y;
            CameraPos.z = _z;
        }
        private void InitTarPos(float _x, float _y, float _z)
        {
            Target.x = _x;
            Target.y = _y;
            Target.z = _z;
        }
        private void MoveCam()
        {
            if(DX.CheckHitKey(DX.KEY_INPUT_UP)!=0)
            {
                CameraPos.z += 1.0f;
            }
            if (DX.CheckHitKey(DX.KEY_INPUT_DOWN) != 0)
            {
                CameraPos.z -= 1.0f;
            }
            if (DX.CheckHitKey(DX.KEY_INPUT_RIGHT) != 0)
            {
                CameraPos.x += 1.0f;
            }
            if (DX.CheckHitKey(DX.KEY_INPUT_LEFT) != 0)
            {
                CameraPos.x -= 1.0f;
            }



        }
        public void Draw()
        {
         //板をぽりごんで描画して、カメラの位置をかえて、斜めにスクロールするようにする。   
           
            DX.ChangeLightTypeDir(DX.VGet(1.0f, 0.0f, 5.0f));
            DX.SetCameraPositionAndTarget_UpVecY(CameraPos,Target);
            DX.SetTextureAddressMode(DX.DX_TEXADDRESS_WRAP);

            if (DX.DrawPolygon3D(out vert[0], 2, Ghandle, DX.FALSE) != 0)
                MessageBox.Show("DrawPolygon3D failed");
            DX.SetTextureAddressMode(DX.DX_TEXADDRESS_CLAMP);
            
            for (int i = 0; i < vert.Length;i++)
            {
                vert[i].u -= 0.001f;
            }
             
            timer++;
        }
    }
}
