using System;
using System.Drawing;
using System.Windows.Forms;

public class ServiceChargeCalculator : Form
{
    
    private Label lblUnitPrice, lblQuantity, lblDiscount, lblResult;
    private TextBox txtUnitPrice, txtQuantity, txtDiscount;
    private Button btnCalculate, btnReset;

    public ServiceChargeCalculator()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        this.Text = "Máy tính cước dịch vụ & Giảm giá";
        this.Size = new Size(450, 320);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Arial", 10);

        int startX = 30;
        int startY = 30;
        int gapY = 40;

     
        lblUnitPrice = new Label { Text = "Đơn giá dịch vụ:", Location = new Point(startX, startY), AutoSize = true };
        txtUnitPrice = new TextBox { Location = new Point(170, startY - 3), Width = 200, TabIndex = 0 };

    
        startY += gapY;
        lblQuantity = new Label { Text = "Số lượng khách:", Location = new Point(startX, startY), AutoSize = true };
        txtQuantity = new TextBox { Location = new Point(170, startY - 3), Width = 200, TabIndex = 1 };

     
        startY += gapY;
        lblDiscount = new Label { Text = "% Giảm giá:", Location = new Point(startX, startY), AutoSize = true };
        txtDiscount = new TextBox { Location = new Point(170, startY - 3), Width = 200, TabIndex = 2 };

        startY += gapY + 15;
        btnCalculate = new Button { Text = "Tính tiền", Location = new Point(120, startY), Size = new Size(100, 35), TabIndex = 3 };
        btnCalculate.Click += BtnCalculate_Click; 

        btnReset = new Button { Text = "Làm mới", Location = new Point(240, startY), Size = new Size(100, 35), TabIndex = 4 };
        btnReset.Click += BtnReset_Click; 

        startY += gapY + 30;
        lblResult = new Label
        {
            Text = "Tổng tiền thanh toán: 0 VNĐ",
            Location = new Point(startX, startY),
            AutoSize = true,
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.DarkRed
        };

        this.Controls.Add(lblUnitPrice); this.Controls.Add(txtUnitPrice);
        this.Controls.Add(lblQuantity); this.Controls.Add(txtQuantity);
        this.Controls.Add(lblDiscount); this.Controls.Add(txtDiscount);
        this.Controls.Add(btnCalculate); this.Controls.Add(btnReset);
        this.Controls.Add(lblResult);
    }

    private void BtnCalculate_Click(object sender, EventArgs e)
    {
        if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
        {
            MessageBox.Show("Vui lòng nhập 'Đơn giá dịch vụ' hợp lệ (số dương).", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUnitPrice.Focus();
            return;
        }

        if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
        {
            MessageBox.Show("Vui lòng nhập 'Số lượng khách' hợp lệ (số nguyên > 0).", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtQuantity.Focus();
            return;
        }

        if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
        {
            MessageBox.Show("Vui lòng nhập '% Giảm giá' hợp lệ (số từ 0 đến 100).", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtDiscount.Focus();
            return;
        }

        decimal total = (unitPrice * quantity) * ((100m - discount) / 100m);

        lblResult.Text = $"Tổng tiền thanh toán: {total:N0} VNĐ";
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        txtUnitPrice.Clear();
        txtQuantity.Clear();
        txtDiscount.Clear();
        lblResult.Text = "Tổng tiền thanh toán: 0 VNĐ";
        
        
        txtUnitPrice.Focus();
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ServiceChargeCalculator());
    }
}