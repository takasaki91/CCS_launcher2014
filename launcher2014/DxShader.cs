using DxLibDLL;
using System;
using System.Drawing;


/*
背景用ピクセルシェーダクラス(C++)
使い方
	CPixelShader ps; //まずピクセルシェーダクラスを宣言します
	ps.LoadShader("aaa.pso"); //ピクセルシェーダをロードします(これの作り方は聞いて下さい)

	(ps.SetRegion(0,0,640,480); //必要な場合は描画サイズを指定(デフォルトはウインドウサイズ) )

	ps.SetShaderConst("constant" , CPixelShader::GetFloat4(0.1,0,0,0) ); //必要ならシェーダに定数を渡す(float4とfloatがあります)
	ps.DrawShaderFromBackGround();	//描画します(勝手に背景を画像ハンドル化して使います)

	ps.DrawShader( grhandle );	//かけたい画像ハンドルが取得出来る場合はこちら(特定のグラフィックハンドルに使う場合や背景を取得せずとも3枚目のバッファなどとして持っている場合)

おまけだいたいIntellisenseで表示してくれますけど関数の説明
GetShaderConstIndex　→　指定の名前の定数(シェーダグローバル定数)の番号を取得(SetShaderConst系関数で使用。float(4)以外の定数を使いたい場合)
GetShaderHandle　→　内部で保持しているシェーダのハンドルを得る(なんか直接弄りたい場合)

*/
//ピクセルシェーダ用クラス

namespace PShader
{
    class HPixelShader
    {
        DX.VERTEX2DSHADER[] Vertex = new DX.VERTEX2DSHADER[6];	//シェーダ用頂点情報
        int ShaderHandle;			//シェーダハンドル

        //コンストラクタ
        public HPixelShader()
        {
            PointF[] defvert = new PointF[4] { new PointF(0.0f, 0.0f) ,new PointF(1.0f, 0.0f) ,
                                           new PointF(0.0f, 1.0f) ,new PointF(1.0f, 1.0f) };
            for (int i = 0; i < 4; i++)
            {
                Vertex[i].pos = DX.VGet(0.0f, 0.0f, 0.0f);	//座標
                Vertex[i].rhw = 1.0f;	//パイプラインの最後に行うスケール変換の値・・・らしい
                Vertex[i].pos.x = Vertex[i].u = Vertex[i].su = defvert[i].X;
                Vertex[i].pos.y = Vertex[i].v = Vertex[i].sv = defvert[i].Y;
                //ディフュージョンカラーおよびスペキュラーカラー
                //ぶっちゃけよくわからない＼(^o^)／
                Vertex[i].dif = DX.GetColorU8(255, 255, 255, 255);
                Vertex[i].spc = DX.GetColorU8(0, 0, 0, 0);
            }
            Vertex[4] = Vertex[2];	//4=2
            Vertex[5] = Vertex[1];	//5=1
            int w, h;
            DX.GetWindowSize(out w, out h);
            SetRegion(0, 0, (float)w, (float)h);
        }
        //UV座標設定(0:左上、1:右上、2：左下,3:右下)
        public void SetUV(int vertnum, float u, float v)
        {
            if (vertnum < 0 || vertnum > 4)
                return;
            Vertex[vertnum].u = Vertex[vertnum].su = u;
            Vertex[vertnum].v = Vertex[vertnum].sv = v;
            if (vertnum == 1)
                Vertex[5] = Vertex[1];
            if (vertnum == 1)
                Vertex[4] = Vertex[2];
        }
        //ピクセルシェーダ(pso)ファイルを読み込む
        public int LoadShader(string filename)
        {
            return (ShaderHandle = DX.LoadPixelShader(filename));
        }
        //シェーダに定数を設定する(グローバル変数に渡る),FLOAT4バージョン
        public void SetShaderConst(string constantname, DX.FLOAT4 value)
        {
            int index = DX.GetConstIndexToShader(constantname, ShaderHandle);
            DX.SetPSConstF(index, value);
        }
        //シェーダに定数を設定する(グローバル変数に渡る),floatバージョン
        public void SetShaderConst(string constantname, float value)
        {
            int index = DX.GetConstIndexToShader(constantname, ShaderHandle);
            DX.SetPSConstSF(index, value);
        }
        //内部で保持しているシェーダのハンドルを得る
        public int GetShaderHandle() { return ShaderHandle; }
        //指定の名前を持つ定数のインデクスを得る。float4,float以外の定数を設定する場合に。
        public int GetShaderConstIndex(string filename) { return DX.GetConstIndexToShader(filename, ShaderHandle); }
        //シェーダをかける範囲を指定する。4点指定
        public void SetRegion(float x, float y, float x2, float y2)
        {
            Vertex[0].pos = DX.VGet(x, y, 0.0f);
            Vertex[1].pos = DX.VGet(x2, y, 0.0f);
            Vertex[2].pos = DX.VGet(x, y2, 0.0f);
            Vertex[3].pos = DX.VGet(x2, y2, 0.0f);
            Vertex[4] = Vertex[2];	//4=2
            Vertex[5] = Vertex[1];	//5=1
        }
        //シェーダをかける範囲を指定する。中心点と大きさ指定
        public void SetRegionC(float cx, float cy, float width, float height, bool reverse = false)
        {
            if (reverse)
            {
                Vertex[0].pos = DX.VGet(cx + width / 2, cy - height / 2, 0.0f);
                Vertex[1].pos = DX.VGet(cx - width / 2, cy - height / 2, 0.0f);
                Vertex[2].pos = DX.VGet(cx + width / 2, cy + height / 2, 0.0f);
                Vertex[3].pos = DX.VGet(cx - width / 2, cy + height / 2, 0.0f);
            }
            else
            {
                Vertex[0].pos = DX.VGet(cx - width / 2, cy - height / 2, 0.0f);
                Vertex[1].pos = DX.VGet(cx + width / 2, cy - height / 2, 0.0f);
                Vertex[2].pos = DX.VGet(cx - width / 2, cy + height / 2, 0.0f);
                Vertex[3].pos = DX.VGet(cx + width / 2, cy + height / 2, 0.0f);
            }
            Vertex[4] = Vertex[2];	//4=2
            Vertex[5] = Vertex[1];	//5=1
        }
        public void Translate(float x, float y)
        {
            for (int i = 0; i < 4; i++)
            {
                Vertex[i].pos.x += x;
                Vertex[i].pos.y += y;
            }
            Vertex[4] = Vertex[2];	//4=2
            Vertex[5] = Vertex[1];	//5=1
        }
        //float4型の値を得る
        public static DX.FLOAT4 GetFloat4(float x, float y, float z, float w)
        {
            DX.FLOAT4 f4;
            f4.x = x;
            f4.y = y;
            f4.z = z;
            f4.w = w;
            return f4;
        }
        //シェーダを描画する
        public int DrawShader(int texture, int stageindex = 0)
        {
            DX.SetUseTextureToShader(stageindex, texture);
            //ピクセルシェーダのセット
            DX.SetUsePixelShader(ShaderHandle);
            //2次元描画
            return DX.DrawPrimitive2DToShader(out Vertex[0], 6, DX.DX_PRIMTYPE_TRIANGLELIST);
        }
        //背景を取得してそこにシェーダを描画する
        public int DrawShaderFromBackGround(int stageindex = 0)
        {
            int w, h;
            DX.GetWindowSize(out w, out h);
            int gr = DX.MakeGraph(w, h);
            DX.GetDrawScreenGraph(0, 0, w, h, gr);
            DrawShader(gr);
            DX.DeleteGraph(gr);
            return 0;
        }
    }
    class Fps
    {
        const int FRAME_EXPECT = 60;
        const double frame1 = 16.6666666;		    //1フレームの理論値
        int[] Measured = new int[FRAME_EXPECT];	//１フレーム経過測定値
        double Ave;							//１フレーム測定平均
        int CountStart;						//1ｓ測定開始時刻、
        int Count;							//測定フレーム（<60)
        int Temp, TempC;					//時間一時記憶用
        int FrameCount;						//経過フレーム
        public int _FrameCount
        {
            get { return FrameCount; }
        }
        public void FrameReset() { FrameCount = 0; }
        public Fps()
        {
            Ave = 1000.0f / FRAME_EXPECT;
            FrameCount = CountStart = Count = Temp = TempC = 0;
            for (int i = 0; i < FRAME_EXPECT; i++)
            {
                Measured[i] = 0;
            }
        }
        public void Wait()
        {
            if (Count == 0)
            {
                if (FrameCount == 0)
                {
                    Temp = 0;
                    TempC = DX.GetNowCount();
                }
                else
                {
                    Temp = (CountStart + 1000) - DX.GetNowCount();
                }
                CountStart = DX.GetNowCount();
            }
            else
            {
                Temp = (int)(CountStart + frame1 * Count) - DX.GetNowCount();
            }

            if (Temp > 0)
            {
                DX.WaitTimer(Temp);	//ウェイト
            }

            Measured[Count] = DX.GetNowCount() - TempC;	//前回との差（測定１フレーム）
            TempC = DX.GetNowCount();	//実行時間を更新
            if (Count == FRAME_EXPECT - 1)	//測定終了
            {
                Ave = 0.0f;
                for (int i = 0; i < FRAME_EXPECT; i++)
                    Ave += (double)Measured[i];
                Ave /= (double)FRAME_EXPECT;
            }

            Count = (++Count) % FRAME_EXPECT;
            FrameCount++;
        }
        public double GetFps() { return 1000.0 / Ave; }	//現在のＦＰＳを返す
    }    
    //やっぱりDrawFormatStringは必要です
    // {0} , {1}と書いていくと後に書いた引数がそこに入る
    // 例： DrawFormatString(0,0,DX.GetColor(255,255,255),"fps = {0}" , fps.GetFPS() );
    public static class DXP
    {
        public static int DrawFormatString(int x, int y, int color, string str, params object[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                str = str.Replace("{" + i.ToString() + "}", args[i].ToString());
            }
            return DX.DrawString(x, y, str, color);
        }

    }
}