using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

// Lớp đối tượng lưu trữ dữ liệu trên RAM
public class VatTu
{
    public string MaVT { get; set; }
    public string TenVT { get; set; }
    public string DonVi { get; set; }
    public decimal DonGia { get; set; }
}

public class ItemListManagerForm : Form
{
    // Cấu trúc lưu trữ RAM
    private List<VatTu> _danhSachVatTu = new List<VatTu>();

    // UI Controls
    private GroupBox grpLeft, grpRight;
    private TextBox txtMaVT, txtTenVT, txtDonGia;
    private ComboBox cboDonVi;
    private Button btnAdd, btnUpdate, btnDelete, btnClearAll;
    private ListView lvItems;

    public ItemListManagerForm()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        this.Text = "Quản lý danh mục Vật tư / Linh kiện";
        this.Size = new Size(850, 450);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Arial", 10);

        // ================= KHUNG TRÁI: NHẬP LIỆU =================
        grpLeft = new GroupBox 
        { 
            Text = "Thông tin Vật tư", 
            Location = new Point(10, 10), 
            Size = new Size(320, 380),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
        };
        this.Controls.Add(grpLeft);

        int startX = 15, startY = 35, gapY = 45;

        grpLeft.Controls.Add(new Label { Text = "Mã vật tư:", Location = new Point(startX, startY), AutoSize = true });
        txtMaVT = new TextBox { Location = new Point(100, startY - 3), Width = 200 };
        grpLeft.Controls.Add(txtMaVT);

        startY += gapY;
        grpLeft.Controls.Add(new Label { Text = "Tên vật tư:", Location = new Point(startX, startY), AutoSize = true });
        txtTenVT = new TextBox { Location = new Point(100, startY - 3), Width = 200 };
        grpLeft.Controls.Add(txtTenVT);

        startY += gapY;
        grpLeft.Controls.Add(new Label { Text = "Đơn vị tính:", Location = new Point(startX, startY), AutoSize = true });
        cboDonVi = new ComboBox { Location = new Point(100, startY - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cboDonVi.Items.AddRange(new string[] { "Cái", "Bộ", "Kg", "Mét" });
        cboDonVi.SelectedIndex = 0;
        grpLeft.Controls.Add(cboDonVi);

        startY += gapY;
        grpLeft.Controls.Add(new Label { Text = "Đơn giá:", Location = new Point(startX, startY), AutoSize = true });
        txtDonGia = new TextBox { Location = new Point(100, startY - 3), Width = 200 };
        grpLeft.Controls.Add(txtDonGia);

        // Các nút chức năng (Height = 35 để không bị mất chữ trên Linux)
        startY += 60;
        btnAdd = new Button { Text = "Thêm mới", Location = new Point(20, startY), Size = new Size(130, 35), BackColor = Color.LightGreen };
        btnAdd.Click += BtnAdd_Click;
        grpLeft.Controls.Add(btnAdd);

        btnUpdate = new Button { Text = "Cập nhật", Location = new Point(160, startY), Size = new Size(130, 35), BackColor = Color.LightBlue };
        btnUpdate.Click += BtnUpdate_Click;
        grpLeft.Controls.Add(btnUpdate);

        startY += 45;
        btnDelete = new Button { Text = "Xóa dòng", Location = new Point(20, startY), Size = new Size(130, 35), BackColor = Color.LightCoral };
        btnDelete.Click += BtnDelete_Click;
        grpLeft.Controls.Add(btnDelete);

        btnClearAll = new Button { Text = "Xóa toàn bộ", Location = new Point(160, startY), Size = new Size(130, 35) };
        btnClearAll.Click += BtnClearAll_Click;
        grpLeft.Controls.Add(btnClearAll);

        // ================= KHUNG PHẢI: DANH SÁCH =================
        grpRight = new GroupBox 
        { 
            Text = "Danh sách Vật tư", 
            Location = new Point(340, 10), 
            Size = new Size(480, 380),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        this.Controls.Add(grpRight);

        lvItems = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(450, 340),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            View = View.Details, // Bật chế độ bảng
            FullRowSelect = true, // Chọn nguyên dòng
            GridLines = true
        };
        
        // Thêm các cột
        lvItems.Columns.Add("Mã VT", 80);
        lvItems.Columns.Add("Tên VT", 160);
        lvItems.Columns.Add("Đơn vị", 80);
        lvItems.Columns.Add("Đơn giá", 120);
        
        // Bắt sự kiện chọn dòng trên ListView
        lvItems.SelectedIndexChanged += LvItems_SelectedIndexChanged;
        
        grpRight.Controls.Add(lvItems);
    }

    // Hàm phụ trợ: Làm sạch TextBox
    private void ClearInput()
    {
        txtMaVT.Clear();
        txtTenVT.Clear();
        txtDonGia.Clear();
        cboDonVi.SelectedIndex = 0;
        txtMaVT.Focus();
    }

    // 1. Chức năng: Thêm mới
    private void BtnAdd_Click(object sender, EventArgs e)
    {
        string ma = txtMaVT.Text.Trim();
        string ten = txtTenVT.Text.Trim();
        
        if (string.IsNullOrEmpty(ma) || string.IsNullOrEmpty(ten))
        {
            MessageBox.Show("Mã và Tên vật tư không được để trống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(txtDonGia.Text, out decimal gia) || gia < 0)
        {
            MessageBox.Show("Đơn giá phải là số hợp lệ >= 0!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Validate: Không cho phép thêm nếu Mã VT đã tồn tại
        if (_danhSachVatTu.Any(v => v.MaVT.Equals(ma, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Mã vật tư này đã tồn tại! Vui lòng nhập mã khác.", "Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Thêm vào RAM
        var vt = new VatTu { MaVT = ma, TenVT = ten, DonVi = cboDonVi.Text, DonGia = gia };
        _danhSachVatTu.Add(vt);

        // Đẩy lên ListView
        ListViewItem item = new ListViewItem(vt.MaVT);
        item.SubItems.Add(vt.TenVT);
        item.SubItems.Add(vt.DonVi);
        item.SubItems.Add(vt.DonGia.ToString("N0"));
        
        lvItems.Items.Add(item);
        ClearInput();
    }

    // 2. Chức năng: Đổ dữ liệu ngược lại khi chọn dòng (Select)
    private void LvItems_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvItems.SelectedItems.Count > 0)
        {
            ListViewItem item = lvItems.SelectedItems[0];
            txtMaVT.Text = item.SubItems[0].Text;
            txtTenVT.Text = item.SubItems[1].Text;
            cboDonVi.Text = item.SubItems[2].Text;
            
            // Xóa dấu phẩy của định dạng N0 để đưa về số thuần túy vào textbox
            txtDonGia.Text = item.SubItems[3].Text.Replace(",", "").Replace(".", ""); 
        }
    }

    // 3. Chức năng: Cập nhật
    private void BtnUpdate_Click(object sender, EventArgs e)
    {
        if (lvItems.SelectedItems.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một dòng để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string maCty = lvItems.SelectedItems[0].Text;
        var vt = _danhSachVatTu.FirstOrDefault(v => v.MaVT == maCty);

        if (vt != null)
        {
            if (!decimal.TryParse(txtDonGia.Text, out decimal gia) || gia < 0)
            {
                MessageBox.Show("Đơn giá phải là số hợp lệ >= 0!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Cập nhật RAM
            vt.TenVT = txtTenVT.Text.Trim();
            vt.DonVi = cboDonVi.Text;
            vt.DonGia = gia;

            // Cập nhật UI ListView
            ListViewItem item = lvItems.SelectedItems[0];
            item.SubItems[1].Text = vt.TenVT;
            item.SubItems[2].Text = vt.DonVi;
            item.SubItems[3].Text = vt.DonGia.ToString("N0");
            
            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // 4. Chức năng: Xóa dòng với xác nhận Yes/No
    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (lvItems.SelectedItems.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một dòng để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa vật tư này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            string ma = lvItems.SelectedItems[0].Text;
            
            // Xóa khỏi RAM
            _danhSachVatTu.RemoveAll(v => v.MaVT == ma);
            
            // Xóa khỏi ListView
            lvItems.SelectedItems[0].Remove();
            ClearInput();
        }
    }

    // 5. Chức năng: Xóa toàn bộ
    private void BtnClearAll_Click(object sender, EventArgs e)
    {
        if (_danhSachVatTu.Count == 0) return;

        DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa TOÀN BỘ danh sách không?", "Cảnh báo nguy hiểm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        
        if (result == DialogResult.Yes)
        {
            _danhSachVatTu.Clear();
            lvItems.Items.Clear();
            ClearInput();
        }
    }

    // Điểm Entry Point để chạy qua Mono
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ItemListManagerForm());
    }
}