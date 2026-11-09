namespace SEOJIN_TMES.Client.Forms.Common
{
    partial class ImagePreviewPopup
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// 디자이너 지원 메서드. 람다식/삼항연산자/문자열보간/객체이니셜라이저/커스텀 static 호출 사용 금지.
        /// 색은 ImagePreviewPopup.cs의 ApplyTheme()에서 런타임에 입힌다.
        ///
        /// 제목 줄 없는 작은 창(FormBorderStyle.None) — 안쪽 여백(Padding 1)이 테두리 역할을 한다.
        /// 구성: imageBox(Fill) 하나뿐.
        /// </summary>
        private void InitializeComponent()
        {
            this.imageBox = new ImageBoxExtensions.ImageBoxEx();
            this.SuspendLayout();
            //
            // imageBox
            //
            this.imageBox.AllowClickZoom = false;
            this.imageBox.AutoPan = false;
            this.imageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageBox.GridDisplayMode = ImageBoxExtensions.ImageBoxGridDisplayMode.None;
            this.imageBox.Location = new System.Drawing.Point(1, 1);
            this.imageBox.Name = "imageBox";
            this.imageBox.ShortcutsEnabled = false;
            this.imageBox.Size = new System.Drawing.Size(398, 298);
            this.imageBox.SizeMode = ImageBoxExtensions.ImageBoxSizeMode.Fit;
            this.imageBox.TabIndex = 0;
            this.imageBox.TabStop = false;
            //
            // ImagePreviewPopup
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.imageBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ImagePreviewPopup";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private ImageBoxExtensions.ImageBoxEx imageBox;
    }
}
