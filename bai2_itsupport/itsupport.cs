using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

public class ITSupportTicketForm : Form
{
    private TextBox txtTicketId, txtRequester;
    private DateTimePicker dtpDate;
    
    private GroupBox grpPriority;
    private RadioButton rdoLow, rdoMedium, rdoHigh;

    private ComboBox cboCategory;
    private GroupBox grpDevices;
    private CheckBox chkPC, chkLaptop, chkPrinter, chkPhone;

    private PictureBox picError;
    private Button btnLoadImage, btnSubmit, btnReset;

    public ITSupportTicketForm()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        this.Text = "Tiếp nhận & Phân loại sự cố IT";
        this.Size = new Size(500, 650);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Arial", 10);

        int startX = 20;
        int startY = 20;
        int gapY = 40;

        // ================= PHẦN 1: THÔNG TIN PHIẾU =================
        this.Controls.Add(new Label { Text = "Mã phiếu:", Location = new Point(startX, startY), AutoSize = true });
        txtTicketId = new TextBox { Location = new Point(140, startY - 3), Width = 300 };
        this.Controls.Add(txtTicketId);

        startY += gapY;
        this.Controls.Add(new Label { Text = "Người yêu cầu:", Location = new Point(startX, startY), AutoSize = true });
        txtRequester = new TextBox { Location = new Point(140, startY - 3), Width = 300 };
        this.Controls.Add(txtRequester);

        startY += gapY;
        this.Controls.Add(new Label { Text = "Ngày ghi nhận:", Location = new Point(startX, startY), AutoSize = true });
        dtpDate = new DateTimePicker { Location = new Point(140, startY - 3), Width = 200, Format = DateTimePickerFormat.Short };
        this.Controls.Add(dtpDate);

        startY += gapY;
        grpPriority = new GroupBox { Text = "Mức độ ưu tiên", Location = new Point(startX, startY), Size = new Size(420, 60) };
        rdoLow = new RadioButton { Text = "Thấp", Location = new Point(20, 25), AutoSize = true, Checked = true };
        rdoMedium = new RadioButton { Text = "Trung bình", Location = new Point(120, 25), AutoSize = true };
        rdoHigh = new RadioButton { Text = "Khẩn cấp", Location = new Point(250, 25), AutoSize = true, ForeColor = Color.Red };
        grpPriority.Controls.AddRange(new Control[] { rdoLow, rdoMedium, rdoHigh });
        this.Controls.Add(grpPriority);

        // ================= PHẦN 2: PHÂN LOẠI & CHI TIẾT =================
        startY += 80;
        this.Controls.Add(new Label { Text = "Loại sự cố:", Location = new Point(startX, startY), AutoSize = true });
        cboCategory = new ComboBox { Location = new Point(140, startY - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cboCategory.Items.AddRange(new string[] { "Phần cứng", "Phần mềm", "Mạng", "Tài khoản" });
        cboCategory.SelectedIndex = 0; 
        this.Controls.Add(cboCategory);

        startY += gapY;
        grpDevices = new GroupBox { Text = "Thiết bị ảnh hưởng", Location = new Point(startX, startY), Size = new Size(420, 80) };
        chkPC = new CheckBox { Text = "Máy tính bàn", Location = new Point(20, 25), AutoSize = true };
        chkLaptop = new CheckBox { Text = "Laptop", Location = new Point(150, 25), AutoSize = true };
        chkPrinter = new CheckBox { Text = "Máy in", Location = new Point(20, 50), AutoSize = true };
        chkPhone = new CheckBox { Text = "Điện thoại", Location = new Point(150, 50), AutoSize = true };
        grpDevices.Controls.AddRange(new Control[] { chkPC, chkLaptop, chkPrinter, chkPhone });
        this.Controls.Add(grpDevices);

        startY += 100;
        this.Controls.Add(new Label { Text = "Ảnh chụp lỗi:", Location = new Point(startX, startY), AutoSize = true });
        
        picError = new PictureBox 
        { 
            Location = new Point(140, startY), 
            Size = new Size(150, 150), 
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.StretchImage  
        };
        this.Controls.Add(picError);

        btnLoadImage = new Button { Text = "Tải ảnh lỗi", Location = new Point(310, startY), Size = new Size(100, 35) };
        btnLoadImage.Click += BtnLoadImage_Click;
        this.Controls.Add(btnLoadImage);

        // ================= PHẦN 3: NÚT CHỨC NĂNG =================
        startY += 170;
        btnSubmit = new Button { Text = "Gửi yêu cầu", Location = new Point(100, startY), Size = new Size(120, 40), BackColor = Color.LightBlue };
        btnSubmit.Click += BtnSubmit_Click;
        this.Controls.Add(btnSubmit);

        btnReset = new Button { Text = "Nhập lại", Location = new Point(240, startY), Size = new Size(120, 40) };
        btnReset.Click += BtnReset_Click;
        this.Controls.Add(btnReset);
    }

    private void BtnLoadImage_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
            ofd.Title = "Chọn ảnh lỗi";
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";  
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                picError.ImageLocation = ofd.FileName;
            }
        }
    }

    private void BtnSubmit_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTicketId.Text) || string.IsNullOrWhiteSpace(txtRequester.Text))
        {
            MessageBox.Show("Vui lòng nhập Mã phiếu và Người yêu cầu!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== TÓM TẮT YÊU CẦU IT =====");
        sb.AppendLine($"- Mã phiếu: {txtTicketId.Text}");
        sb.AppendLine($"- Người yêu cầu: {txtRequester.Text}");
        sb.AppendLine($"- Ngày ghi nhận: {dtpDate.Value.ToString("dd/MM/yyyy")}");
        
        string priority = rdoLow.Checked ? "Thấp" : (rdoMedium.Checked ? "Trung bình" : "Khẩn cấp");
        sb.AppendLine($"- Mức độ ưu tiên: {priority}");
        
        sb.AppendLine($"- Loại sự cố: {cboCategory.SelectedItem.ToString()}");
        
        sb.Append("- Thiết bị ảnh hưởng: ");
        bool hasDevice = false;
        if (chkPC.Checked) { sb.Append("Máy tính bàn, "); hasDevice = true; }
        if (chkLaptop.Checked) { sb.Append("Laptop, "); hasDevice = true; }
        if (chkPrinter.Checked) { sb.Append("Máy in, "); hasDevice = true; }
        if (chkPhone.Checked) { sb.Append("Điện thoại, "); hasDevice = true; }
        
        if (!hasDevice) sb.Append("Không có");
        else sb.Length -= 2; 
        
        sb.AppendLine(); 
        sb.AppendLine($"- Có ảnh đính kèm: {(picError.ImageLocation != null ? "Có" : "Không")}");

        MessageBox.Show(sb.ToString(), "Thông tin Phiếu Hỗ Trợ", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        txtTicketId.Clear();
        txtRequester.Clear();
        dtpDate.Value = DateTime.Now;
        
        rdoLow.Checked = true;
        
        cboCategory.SelectedIndex = 0;
        
        chkPC.Checked = false;
        chkLaptop.Checked = false;
        chkPrinter.Checked = false;
        chkPhone.Checked = false;
        
        picError.Image = null;
        picError.ImageLocation = null;

        txtTicketId.Focus();
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ITSupportTicketForm());
    }
}