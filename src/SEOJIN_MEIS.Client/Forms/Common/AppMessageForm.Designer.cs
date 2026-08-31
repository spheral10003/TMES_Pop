namespace SEOJIN_MEIS.Client.Forms.Common
{
    partial class AppMessageForm
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
        /// 이 폼은 <see cref="AppMessagePanel"/>을 감싸 모달로 띄우는 껍데기다 —
        /// 실제 내용(메시지/버튼/색)은 AppMessageForm.cs의 Initialize()에서 패널에 넣는다.
        /// </summary>
        private void InitializeComponent()
        {
            this.borderPanel = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.borderPanel)).BeginInit();
            this.SuspendLayout();
            //
            // borderPanel
            //
            this.borderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.borderPanel.Location = new System.Drawing.Point(0, 0);
            this.borderPanel.Name = "borderPanel";
            this.borderPanel.Padding = new System.Windows.Forms.Padding(0);
            this.borderPanel.Size = new System.Drawing.Size(420, 180);
            this.borderPanel.TabIndex = 0;
            //
            // AppMessageForm
            //
            this.ClientSize = new System.Drawing.Size(420, 180);
            this.Controls.Add(this.borderPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppMessageForm";
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AppMessageForm";
            ((System.ComponentModel.ISupportInitialize)(this.borderPanel)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl borderPanel;
    }
}
