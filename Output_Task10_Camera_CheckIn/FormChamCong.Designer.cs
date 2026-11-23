namespace QuanLyChamCong
{
    partial class FormChamCong
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCheckIn = new Button();
            btnCheckOut = new Button();
            btnLichSu = new Button();
            btnDangXuat = new Button();
            lblTieuDe = new Label();
            lblTenNV = new Label();
            lblMaNV = new Label();
            lblChucVu = new Label();
            lblCamera = new Label();
            cboCamera = new ComboBox();
            btnBatTatCamera = new Button();
            picCamera = new PictureBox();
            lblTrangThaiCamera = new Label();
            ((System.ComponentModel.ISupportInitialize)picCamera).BeginInit();
            SuspendLayout();
            // 
            // btnCheckIn
            // 
            btnCheckIn.Location = new Point(56, 191);
            btnCheckIn.Name = "btnCheckIn";
            btnCheckIn.Size = new Size(412, 43);
            btnCheckIn.TabIndex = 0;
            btnCheckIn.Text = "Chấm công vào";
            btnCheckIn.UseVisualStyleBackColor = true;
            btnCheckIn.Click += btnCheckIn_Click;
            // 
            // btnCheckOut
            // 
            btnCheckOut.Location = new Point(56, 240);
            btnCheckOut.Name = "btnCheckOut";
            btnCheckOut.Size = new Size(412, 43);
            btnCheckOut.TabIndex = 1;
            btnCheckOut.Text = "Chấm công ra";
            btnCheckOut.UseVisualStyleBackColor = true;
            btnCheckOut.Click += btnCheckOut_Click;
            // 
            // btnLichSu
            // 
            btnLichSu.Location = new Point(56, 289);
            btnLichSu.Name = "btnLichSu";
            btnLichSu.Size = new Size(412, 43);
            btnLichSu.TabIndex = 2;
            btnLichSu.Text = "Xem lịch sử chấm công";
            btnLichSu.UseVisualStyleBackColor = true;
            btnLichSu.Click += btnLichSu_Click;
            // 
            // btnDangXuat
            // 
            btnDangXuat.Location = new Point(56, 338);
            btnDangXuat.Name = "btnDangXuat";
            btnDangXuat.Size = new Size(412, 43);
            btnDangXuat.TabIndex = 3;
            btnDangXuat.Text = "Đăng xuất";
            btnDangXuat.UseVisualStyleBackColor = true;
            btnDangXuat.Click += btnDangXuat_Click;
            // 
            // lblTieuDe
            // 
            lblTieuDe.AutoSize = true;
            lblTieuDe.Location = new Point(198, 21);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(136, 20);
            lblTieuDe.TabIndex = 4;
            lblTieuDe.Text = "Quản lý chấm công";
            // 
            // lblTenNV
            // 
            lblTenNV.AutoSize = true;
            lblTenNV.Location = new Point(56, 72);
            lblTenNV.Name = "lblTenNV";
            lblTenNV.Size = new Size(75, 20);
            lblTenNV.TabIndex = 5;
            lblTenNV.Text = "Nhân viên";
            // 
            // lblMaNV
            // 
            lblMaNV.AutoSize = true;
            lblMaNV.Location = new Point(56, 114);
            lblMaNV.Name = "lblMaNV";
            lblMaNV.Size = new Size(97, 20);
            lblMaNV.TabIndex = 6;
            lblMaNV.Text = "Mã nhân viên";
            // 
            // lblChucVu
            // 
            lblChucVu.AutoSize = true;
            lblChucVu.Location = new Point(56, 154);
            lblChucVu.Name = "lblChucVu";
            lblChucVu.Size = new Size(61, 20);
            lblChucVu.TabIndex = 7;
            lblChucVu.Text = "Chức vụ";
            // 
            // lblCamera
            // 
            lblCamera.AutoSize = true;
            lblCamera.Location = new Point(500, 52);
            lblCamera.Name = "lblCamera";
            lblCamera.Size = new Size(96, 20);
            lblCamera.TabIndex = 8;
            lblCamera.Text = "Chọn camera:";
            // 
            // cboCamera
            // 
            cboCamera.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCamera.FormattingEnabled = true;
            cboCamera.Location = new Point(500, 76);
            cboCamera.Name = "cboCamera";
            cboCamera.Size = new Size(235, 28);
            cboCamera.TabIndex = 9;
            cboCamera.SelectedIndexChanged += cboCamera_SelectedIndexChanged;
            // 
            // btnBatTatCamera
            // 
            btnBatTatCamera.Location = new Point(745, 75);
            btnBatTatCamera.Name = "btnBatTatCamera";
            btnBatTatCamera.Size = new Size(95, 29);
            btnBatTatCamera.TabIndex = 10;
            btnBatTatCamera.Text = "Bật Camera";
            btnBatTatCamera.UseVisualStyleBackColor = true;
            btnBatTatCamera.Click += btnBatTatCamera_Click;
            // 
            // picCamera
            // 
            picCamera.BackColor = Color.Black;
            picCamera.BorderStyle = BorderStyle.FixedSingle;
            picCamera.Location = new Point(500, 114);
            picCamera.Name = "picCamera";
            picCamera.Size = new Size(340, 240);
            picCamera.SizeMode = PictureBoxSizeMode.Zoom;
            picCamera.TabIndex = 11;
            picCamera.TabStop = false;
            // 
            // lblTrangThaiCamera
            // 
            lblTrangThaiCamera.AutoSize = false;
            lblTrangThaiCamera.Location = new Point(500, 364);
            lblTrangThaiCamera.Name = "lblTrangThaiCamera";
            lblTrangThaiCamera.Size = new Size(340, 22);
            lblTrangThaiCamera.TabIndex = 12;
            lblTrangThaiCamera.Text = "Camera: Đang khởi tạo...";
            // 
            // FormChamCong
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(880, 450);
            Controls.Add(lblTrangThaiCamera);
            Controls.Add(picCamera);
            Controls.Add(btnBatTatCamera);
            Controls.Add(cboCamera);
            Controls.Add(lblCamera);
            Controls.Add(lblChucVu);
            Controls.Add(lblMaNV);
            Controls.Add(lblTenNV);
            Controls.Add(lblTieuDe);
            Controls.Add(btnDangXuat);
            Controls.Add(btnLichSu);
            Controls.Add(btnCheckOut);
            Controls.Add(btnCheckIn);
            Name = "FormChamCong";
            Text = "FormChamCong";
            FormClosing += FormChamCong_FormClosing;
            Load += FormChamCong_Load;
            ((System.ComponentModel.ISupportInitialize)picCamera).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCheckIn;
        private Button btnCheckOut;
        private Button btnLichSu;
        private Button btnDangXuat;
        private Label lblTieuDe;
        private Label lblTenNV;
        private Label lblMaNV;
        private Label lblChucVu;
        private Label lblCamera;
        private ComboBox cboCamera;
        private Button btnBatTatCamera;
        private PictureBox picCamera;
        private Label lblTrangThaiCamera;
    }
}
