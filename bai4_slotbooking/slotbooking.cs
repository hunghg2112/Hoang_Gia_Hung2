using System;
using System.Drawing;
using System.Windows.Forms;

public class SlotBookingForm : Form
{
    // Khai báo Controls
    private ComboBox cboTime;
    private Label lblCount, lblTotal;
    private FlowLayoutPanel flpGrid;
    private Button btnConfirm, btnCancel;

    // Các hằng số giá tiền
    private const decimal GIA_SANG = 100000m;
    private const decimal GIA_TOI = 150000m;

    public SlotBookingForm()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        this.Text = "Sơ đồ Đặt bàn / Chỗ ngồi (Interactive Booking)";
        this.Size = new Size(500, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Arial", 10);

        // ================= PHẦN 1: THANH CÔNG CỤ (TOP) =================
        this.Controls.Add(new Label { Text = "Khung giờ:", Location = new Point(20, 23), AutoSize = true });
        
        cboTime = new ComboBox { Location = new Point(100, 20), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        cboTime.Items.AddRange(new string[] { "Sáng (100.000đ)", "Tối (150.000đ)" });
        cboTime.SelectedIndex = 0;
        cboTime.SelectedIndexChanged += (s, e) => UpdateStats(); // Đổi khung giờ tự tính lại tiền
        this.Controls.Add(cboTime);

        lblCount = new Label { Text = "Số vị trí đang chọn: 0", Location = new Point(280, 15), AutoSize = true, ForeColor = Color.Blue };
        lblTotal = new Label { Text = "Tạm tính: 0 VNĐ", Location = new Point(280, 35), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Red };
        
        this.Controls.Add(lblCount);
        this.Controls.Add(lblTotal);

        // ================= PHẦN 2: MA TRẬN BÀN (MIDDLE) =================
        // Dùng FlowLayoutPanel để nó tự động bẻ dòng thành ma trận 4x5
        flpGrid = new FlowLayoutPanel
        {
            Location = new Point(25, 80),
            Size = new Size(435, 270), // Đủ rộng cho 5 nút ngang (80*5) và 4 nút dọc (60*4)
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };
        this.Controls.Add(flpGrid);

        // Tự động sinh 20 Button ngay khi Form khởi chạy
        for (int i = 1; i <= 20; i++)
        {
            Button btnTable = new Button
            {
                Text = $"{i}",
                Size = new Size(70, 50),
                Margin = new Padding(5),
                BackColor = Color.LightGray,
                Cursor = Cursors.Hand,
                // Dùng thuộc tính Tag để lưu trạng thái: 0 = Trống, 1 = Đang chọn, 2 = Đã khóa
                Tag = 0 
            };

            // Gán CHUNG 1 hàm xử lý sự kiện Click cho cả 20 nút
            btnTable.Click += TableButton_Click;
            
            flpGrid.Controls.Add(btnTable);
        }

        // ================= PHẦN 3: NÚT CHỨC NĂNG (BOTTOM) =================
        btnConfirm = new Button { Text = "Xác nhận đặt", Location = new Point(100, 380), Size = new Size(130, 45), BackColor = Color.LightGreen };
        btnConfirm.Click += BtnConfirm_Click;
        this.Controls.Add(btnConfirm);

        btnCancel = new Button { Text = "Hủy chọn tất cả", Location = new Point(250, 380), Size = new Size(130, 45), BackColor = Color.LightCoral };
        btnCancel.Click += BtnCancel_Click;
        this.Controls.Add(btnCancel);
    }

    // ================= EVENT AGGREGATION: Xử lý click cho 20 nút =================
    private void TableButton_Click(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        if (btn == null) return;

        int currentState = (int)btn.Tag;

        if (currentState == 2)
        {
            MessageBox.Show($"Vị trí số {btn.Text} đã có người đặt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (currentState == 0) // Đang trống -> Chuyển sang Đang chọn
        {
            btn.Tag = 1;
            btn.BackColor = Color.LightGreen;
        }
        else if (currentState == 1) // Đang chọn -> Chuyển lại thành Trống
        {
            btn.Tag = 0;
            btn.BackColor = Color.LightGray;
        }

        UpdateStats(); // Gọi hàm cập nhật số lượng và tiền realtime
    }

    // ================= HÀM HỖ TRỢ: Thống kê & Tính tiền =================
    private void UpdateStats()
    {
        int count = 0;
        
        // Quét toàn bộ nút trong lưới để đếm xem có bao nhiêu nút đang ở trạng thái 1 (Đang chọn)
        foreach (Control ctrl in flpGrid.Controls)
        {
            if (ctrl is Button btn && (int)btn.Tag == 1)
            {
                count++;
            }
        }

        lblCount.Text = $"Số vị trí đang chọn: {count}";

        decimal pricePerSlot = cboTime.SelectedIndex == 0 ? GIA_SANG : GIA_TOI;
        decimal total = count * pricePerSlot;

        lblTotal.Text = $"Tạm tính: {total:N0} VNĐ";
    }

    // ================= SỰ KIỆN: Xác nhận đặt =================
    private void BtnConfirm_Click(object sender, EventArgs e)
    {
        int count = 0;
        foreach (Control ctrl in flpGrid.Controls)
        {
            if (ctrl is Button btn && (int)btn.Tag == 1)
            {
                btn.Tag = 2; // Chuyển sang trạng thái Đã khóa
                btn.BackColor = Color.Salmon; // Chuyển màu Đỏ/Hồng
                count++;
            }
        }

        if (count > 0)
        {
            MessageBox.Show($"Bạn đã đặt thành công {count} chỗ!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateStats(); // Cập nhật lại nhãn sau khi đặt
        }
        else
        {
            MessageBox.Show("Bạn chưa chọn vị trí nào để đặt!", "Lưu ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ================= SỰ KIỆN: Hủy chọn tất cả =================
    private void BtnCancel_Click(object sender, EventArgs e)
    {
        foreach (Control ctrl in flpGrid.Controls)
        {
            // Chỉ hủy những bàn đang ở trạng thái Đang chọn (Tag = 1), không đụng tới bàn Đã đặt (Tag = 2)
            if (ctrl is Button btn && (int)btn.Tag == 1)
            {
                btn.Tag = 0;
                btn.BackColor = Color.LightGray;
            }
        }
        UpdateStats();
    }

    // Điểm Entry Point để chạy qua Mono
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SlotBookingForm());
    }
}