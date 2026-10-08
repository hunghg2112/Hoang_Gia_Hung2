using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

// ==========================================
// MÔ HÌNH DỮ LIỆU
// ==========================================
public class OrderItem
{
    public string TenHang { get; set; } = "";
    public int SoLuong { get; set; } = 0;
    public double TrongLuong { get; set; } = 0;
    public decimal DonGia { get; set; } = 0;
    public decimal ThanhTien => SoLuong * DonGia; // Tự tính Thành tiền
}

// ==========================================
// GIAO DIỆN CHÍNH
// ==========================================
public class DeliveryDashboardForm : Form
{
    // Layout Controls
    private SplitContainer splitContainer;
    private GroupBox grpInfo, grpDetails;
    
    // Left Panel Controls
    private TextBox txtCustomer, txtPhone, txtAddress;
    private ComboBox cboShipping;
    
    // Right Panel Controls
    private DataGridView dgvItems;
    private BindingList<OrderItem> _orderList;
    
    // Status Controls & Utilities
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblTime, lblTotalQty, lblTotalWeight, lblTotalMoney;
    private Timer sysTimer;
    private ErrorProvider errorProvider;

    public DeliveryDashboardForm()
    {
        SetupUI();
        InitializeData();
        WireEvents();
    }

    private void SetupUI()
    {
        this.Text = "Bảng điều khiển Quản lý Đơn giao hàng (Delivery Dashboard)";
        this.Size = new Size(1100, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Arial", 10);
        this.KeyPreview = true; // Bắt buộc bật để Form nhận phím tắt F2 / Delete

        // 1. SplitContainer chia 2 cột
        splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 320,
            FixedPanel = FixedPanel.Panel1
        };
        this.Controls.Add(splitContainer);

        // ================= CỘT TRÁI (THÔNG TIN KHÁCH HÀNG) =================
        grpInfo = new GroupBox { Text = "Thông tin Khách hàng & Vận chuyển", Dock = DockStyle.Fill, Padding = new Padding(10) };
        splitContainer.Panel1.Controls.Add(grpInfo);

        int y = 35;
        grpInfo.Controls.Add(new Label { Text = "Tên khách hàng:", Location = new Point(15, y), AutoSize = true });
        txtCustomer = new TextBox { Location = new Point(130, y - 3), Width = 170 };
        grpInfo.Controls.Add(txtCustomer);

        y += 45;
        grpInfo.Controls.Add(new Label { Text = "Số điện thoại:", Location = new Point(15, y), AutoSize = true });
        txtPhone = new TextBox { Location = new Point(130, y - 3), Width = 170 };
        grpInfo.Controls.Add(txtPhone);

        y += 45;
        grpInfo.Controls.Add(new Label { Text = "Địa chỉ giao:", Location = new Point(15, y), AutoSize = true });
        txtAddress = new TextBox { Location = new Point(130, y - 3), Width = 170, Multiline = true, Height = 60 };
        grpInfo.Controls.Add(txtAddress);

        y += 85;
        grpInfo.Controls.Add(new Label { Text = "Loại vận chuyển:", Location = new Point(15, y), AutoSize = true });
        cboShipping = new ComboBox { Location = new Point(130, y - 3), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
        cboShipping.Items.AddRange(new string[] { "Hỏa tốc", "Tiêu chuẩn", "Tiết kiệm" });
        cboShipping.SelectedIndex = 1;
        grpInfo.Controls.Add(cboShipping);

        Label lblHelp = new Label 
        { 
            Text = "Phím tắt:\n[F2] - Thêm dòng mới\n[Delete] - Xóa dòng chọn", 
            Location = new Point(15, y + 80), 
            AutoSize = true, 
            ForeColor = Color.Blue 
        };
        grpInfo.Controls.Add(lblHelp);

        // ================= CỘT PHẢI (CHI TIẾT HÀNG HÓA) =================
        grpDetails = new GroupBox { Text = "Danh mục Hàng hóa (Nhập trực tiếp vào lưới)", Dock = DockStyle.Fill, Padding = new Padding(10) };
        splitContainer.Panel2.Controls.Add(grpDetails);

        dgvItems = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false, // Vô hiệu hóa dòng trắng dưới cùng, dùng F2 để thêm
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };

        // Khởi tạo các cột
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenHang", HeaderText = "Tên hàng hóa" });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SoLuong", HeaderText = "Số lượng" });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TrongLuong", HeaderText = "Trọng lượng (kg)" });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DonGia", HeaderText = "Đơn giá", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            DataPropertyName = "ThanhTien", 
            HeaderText = "Thành tiền", 
            ReadOnly = true, // Khóa không cho sửa cột Thành Tiền
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", BackColor = Color.LightGray, Font = new Font("Arial", 10, FontStyle.Bold) }
        });

        grpDetails.Controls.Add(dgvItems);

        // ================= THANH TRẠNG THÁI (STATUS STRIP) =================
        statusStrip = new StatusStrip();
        lblTime = new ToolStripStatusLabel { ForeColor = Color.DarkBlue, Margin = new Padding(0, 0, 20, 0) };
        lblTotalQty = new ToolStripStatusLabel { Margin = new Padding(0, 0, 20, 0) };
        lblTotalWeight = new ToolStripStatusLabel { Margin = new Padding(0, 0, 20, 0) };
        lblTotalMoney = new ToolStripStatusLabel { ForeColor = Color.Red, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        
        statusStrip.Items.AddRange(new ToolStripItem[] { lblTime, lblTotalQty, lblTotalWeight, lblTotalMoney });
        this.Controls.Add(statusStrip);

        errorProvider = new ErrorProvider(this);
    }

    private void InitializeData()
    {
        _orderList = new BindingList<OrderItem>();
        
        // Thêm dữ liệu mẫu ban đầu
        _orderList.Add(new OrderItem { TenHang = "Bàn phím cơ", SoLuong = 2, TrongLuong = 1.5, DonGia = 850000 });
        _orderList.Add(new OrderItem { TenHang = "Chuột không dây", SoLuong = 5, TrongLuong = 0.8, DonGia = 320000 });
        
        dgvItems.DataSource = _orderList;
        UpdateStatusTotals();

        // Cấu hình Timer chạy Đồng hồ thời gian thực
        sysTimer = new Timer { Interval = 1000 };
        sysTimer.Tick += (s, e) => lblTime.Text = "🕒 " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        sysTimer.Start();
    }

    private void WireEvents()
    {
        // Khi thay đổi giá trị ô => Tính lại thành tiền
        dgvItems.CellValueChanged += (s, e) => 
        {
            UpdateStatusTotals();
            dgvItems.Invalidate(); // Refresh lưới để cập nhật cột Thành tiền
        };

        // Bắt sự kiện chỉnh sửa (Edit Control) để gắn ErrorProvider
        dgvItems.EditingControlShowing += DgvItems_EditingControlShowing;
    }

    // ================= XỬ LÝ SỰ KIỆN: Tính toán Realtime =================
    private void UpdateStatusTotals()
    {
        int totalQty = _orderList.Sum(x => x.SoLuong);
        double totalWeight = _orderList.Sum(x => x.TrongLuong);
        decimal totalMoney = _orderList.Sum(x => x.ThanhTien);

        lblTotalQty.Text = $"📦 Tổng số lượng: {totalQty}";
        lblTotalWeight.Text = $"⚖️ Tổng trọng lượng: {totalWeight:0.0} kg";
        lblTotalMoney.Text = $"💰 Tổng tiền: {totalMoney:N0} VNĐ";
    }

    // ================= XỬ LÝ SỰ KIỆN: Validation với ErrorProvider =================
    private void DgvItems_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
    {
        // Lấy Control TextBox mà DataGridView đang dùng để nhập liệu
        if (e.Control is TextBox tb)
        {
            tb.TextChanged -= CellTextBox_TextChanged; // Gỡ sự kiện cũ tránh lặp
            tb.TextChanged += CellTextBox_TextChanged; // Gắn sự kiện kiểm tra trực tiếp
        }
    }

    private void CellTextBox_TextChanged(object sender, EventArgs e)
    {
        TextBox tb = sender as TextBox;
        int colIndex = dgvItems.CurrentCell.ColumnIndex;

        // Cột 1 là Số lượng, Cột 2 là Trọng lượng
        if (colIndex == 1) 
        {
            if (int.TryParse(tb.Text, out int val) && val <= 0)
                errorProvider.SetError(tb, "Số lượng phải lớn hơn 0!");
            else
                errorProvider.SetError(tb, ""); // Xóa lỗi
        }
        else if (colIndex == 2)
        {
            if (double.TryParse(tb.Text, out double val) && val <= 0)
                errorProvider.SetError(tb, "Trọng lượng phải lớn hơn 0!");
            else
                errorProvider.SetError(tb, "");
        }
    }

    // ================= XỬ LÝ SỰ KIỆN: Phím tắt F2 và Delete =================
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Nhấn F2: Thêm dòng mới
        if (keyData == Keys.F2)
        {
            _orderList.Add(new OrderItem { TenHang = "Hàng mới...", SoLuong = 1, TrongLuong = 1.0, DonGia = 0 });
            dgvItems.CurrentCell = dgvItems.Rows[dgvItems.Rows.Count - 1].Cells[0]; // Trỏ trỏ chuột về dòng mới
            UpdateStatusTotals();
            return true; 
        }
        // Nhấn Delete: Xóa dòng đang chọn
        else if (keyData == Keys.Delete)
        {
            // Chỉ xóa khi đang chọn cả dòng (không phải đang gõ chữ trong 1 ô)
            if (dgvItems.CurrentRow != null && !dgvItems.IsCurrentCellInEditMode)
            {
                _orderList.RemoveAt(dgvItems.CurrentRow.Index);
                UpdateStatusTotals();
                return true;
            }
        }
        
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // Entry Point cho Mono/Linux
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DeliveryDashboardForm());
    }
}