using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QuanLyLinhKienDIenTu.Login;
using static QuanLyLinhKienDIenTu.Order;

namespace QuanLyLinhKienDIenTu
{
    public partial class Cart : Form
    {

        string strCon = Connecting.GetConnectionString();
        public static int CustomerID;
        public static int ProductID;
        int UserId = UserSession.UserId;
        string Role = UserSession.role;
        public MainForm mf;
        public Cart(MainForm mainForm)
        {
            InitializeComponent();
            mf = mainForm;
        }

        private void Salary_Load(object sender, EventArgs e)
        {

            Round.SetSharpCornerPanel(PanelDel);
            Round.SetSharpCornerPanel(panel_Acc);
            LoadCartData(CustomerID);
            DataGrindview();
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            dataGridView1.CellClick += DataGridView1_CellClick;
            tableLayoutPanel1.Visible = false;
            btnOrder.Enabled = false;
            btnDel.Enabled = false;
            if (CountItemsInCart() > 0)
            {
                btnOrder.Enabled = true;
                btnDel.Enabled = true;
            }
            if(Role != "Khach Hang")
            {
                panel_Acc.Visible = false;
            }
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
        }
        private bool isLoadingData = false;

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!isLoadingData && e.ColumnIndex == dataGridView1.Columns["SOLUONG"].Index && e.RowIndex >= 0)
            {
                int newQuantity;
                if (int.TryParse(dataGridView1.Rows[e.RowIndex].Cells["SOLUONG"].Value.ToString(), out newQuantity))
                {
                    int cartItemId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["MASANPHAM"].Value);
                    UpdateCartItemQuantity(cartItemId, newQuantity);

                    // Gọi lại LoadCartData bằng BeginInvoke để tránh lỗi reentrant
                    BeginInvoke((MethodInvoker)delegate {
                        LoadCartData(CustomerID); // Cập nhật lại dữ liệu trong giỏ hàng sau khi cập nhật số lượng
                    });
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập số lượng hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // Nếu không thành công, bạn có thể giữ nguyên số lượng cũ hoặc xử lý theo cách khác
                }
            }
        }


        int CartID;
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Lấy giá trị cart_item_id từ hàng được chọn
                CartID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["MASANPHAM"].Value);

            }
        }


        //Xóa sản phẩm khỏi giỏ hàng
        private void DeleteCartItem(int cartItemId)
        {
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand("DELETE FROM GIOHANG WHERE MASANPHAM = @CartItemID", connection))
                {
                    // Thêm tham số cho câu lệnh DELETE
                    command.Parameters.AddWithValue("@CartItemID", cartItemId);

                    connection.Open();
                    // Thực thi câu lệnh DELETE
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Đã xóa sản phẩm khỏi giỏ hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm trong giỏ hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void DataGrindview()
        {
            // Ẩn hàng dự thêm mới ở cuối DataGridView
            dataGridView1.AllowUserToAddRows = false;
            // Đặt DataGridView thành chỉ chế độ chỉ đọc
            dataGridView1.ReadOnly = true;
            dataGridView1.BackgroundColor = this.BackColor;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.RowHeadersVisible = false;
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        private void UpdateCartItemQuantity(int cartItemId, int newQuantity)
        {
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand("UPDATE GIOHANG SET SOLUONG = @NewQuantity WHERE MASANPHAM = @CartItemID", connection))
                {
                    command.Parameters.AddWithValue("@NewQuantity", newQuantity);
                    command.Parameters.AddWithValue("@CartItemID", cartItemId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Đã cập nhật số lượng sản phẩm trong giỏ hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm trong giỏ hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }


        //double click event cần sữa
        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "SOLUONG")
            {
                dataGridView1.ReadOnly = false;
                dataGridView1.Rows[e.RowIndex].Cells["MAGH"].ReadOnly = true;
                dataGridView1.Rows[e.RowIndex].Cells["MASANPHAM"].ReadOnly = true;
                dataGridView1.Rows[e.RowIndex].Cells["TENSANPHAM"].ReadOnly = true;
                dataGridView1.Rows[e.RowIndex].Cells["GIATIEN"].ReadOnly = true;

                dataGridView1.Rows[e.RowIndex].Cells["SOLUONG"].ReadOnly = false;
                dataGridView1.BeginEdit(true);
                dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
            }
        }

        //Load dữ liệu giỏ hàn của khách hàng
        private void LoadCartData(int CusID)
        {
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand("GetProductNameForCart", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerID", CusID);

                    connection.Open();

                    // Thực thi stored procedure và lấy dữ liệu vào DataTable
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Kiểm tra xem khách hàng có giỏ hàng không
                    if (dataTable.Rows.Count == 0)
                    {
                        dataGridView1.Columns.Clear();

                        // Thêm các cột mới với tiêu đề tương ứng
                        dataGridView1.Columns.Add("MAGH", "Mã giỏ hàng");
                        dataGridView1.Columns.Add("MASANPHAM", "Mã sản phẩm");
                        dataGridView1.Columns.Add("TENSANPHAM", "Tên sản phẩm");
                        dataGridView1.Columns.Add("SOLUONG", "Số lượng");
                        dataGridView1.Columns.Add("GIATIEN", "Đơn giá");
                    }
                    else
                    {
                        isLoadingData = true; // Đánh dấu đang tải dữ liệu để tránh lỗi reentrant
                                              // Gán DataTable cho DataSource của DataGridView
                        dataGridView1.DataSource = dataTable;

                        // Đặt lại tiêu đề cho các cột
                        dataGridView1.Columns["MAGH"].HeaderText = "Mã giỏ hàng";
                        dataGridView1.Columns["TENSANPHAM"].HeaderText = "Tên sản phẩm";
                        dataGridView1.Columns["MASANPHAM"].HeaderText = "Mã sản phẩm";
                        dataGridView1.Columns["SOLUONG"].HeaderText = "Số lượng";
                        dataGridView1.Columns["GIATIEN"].HeaderText = "Đơn giá";
                        isLoadingData = false; // Kết thúc tải dữ liệu
                    }
                }
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if(CartID != default(int))
            {
                DeleteCartItem(CartID);
                LoadCartData(CustomerID);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn sản phẩm muốn xóa khổi giỏ hàng");
            }
        }

        bool OrderMode = true;
        bool RunDone = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if(CountItemsInCart() > 0)
            {
                if (OrderMode)
                {
                    // Chuyển sang chế độ xác nhận đặt hàng
                    btnOrder.Text = "  Xác nhận";
                    OrderMode = false;
                    tableLayoutPanel1.Visible = true;
                }
                else
                {
                    // Kiểm tra xem các TextBox có rỗng không
                    if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtAddress.Text))
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ thông tin tên và địa chỉ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Thoát khỏi phương thức nếu có một trong hai TextBox rỗng
                    }
                    DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đặt hàng?", "Xác nhận đặt hàng", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        InsertOrder(CustomerID);
                        if (RunDone)
                        {
                            LoadCartData(CustomerID);
                            mf.OpenChildForm(new ViewOrder(mf));
                        }
                    }
                    else
                    {
                        // Chuyển sang chế độ xem giỏ hàng
                        btnOrder.Text = "Đặt hàng";
                        OrderMode = true;
                        tableLayoutPanel1.Visible = false;
                    }
                }
            }
        }

        // Hàm đếm số lượng mặt hàng trong giỏ hàng
        private int CountItemsInCart()
        {
            int count = 0;
            // Thực hiện truy vấn để đếm số lượng mặt hàng trong giỏ hàng
            // Đoạn code này phụ thuộc vào cách bạn thiết kế hệ thống và cách truy vấn dữ liệu từ cơ sở dữ liệu
            // Dưới đây là một ví dụ giả định
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                string query = "SELECT COUNT(*) " +
                               "FROM GIOHANG INNER JOIN KHACHHANG " +
                               "ON GIOHANG.MAKH = KHACHHANG.MAKH " +
                               "WHERE KHACHHANG.MAND = @CustomerID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", CustomerID);
                    connection.Open();
                    count = (int)command.ExecuteScalar();
                }
            }
            return count;
        }


        //Chuyển dữ liệu từ giỏ hàng sang table đặt hàng và hóa đơn
        public void InsertOrder(int CusID)
        {
            string recipientName = txtName.Text;
            string deliveryAddress = txtAddress.Text; 

            try
            {
                // Tạo kết nối đến cơ sở dữ liệu
                using (SqlConnection connection = new SqlConnection(strCon))
                {
                    // Mở kết nối
                    connection.Open();

                    // Tạo SqlCommand để gọi stored procedure InsertOrder
                    using (SqlCommand command = new SqlCommand("InsertOrder", connection))
                    {
                        // Đặt CommandType là StoredProcedure
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm các tham số vào stored procedure
                        command.Parameters.AddWithValue("@MAND", CusID);
                        command.Parameters.AddWithValue("@RecipientName", recipientName);
                        command.Parameters.AddWithValue("@DeliveryAddress", deliveryAddress);

                        // Thực thi stored procedure
                        command.ExecuteNonQuery();

                        // Hiển thị thông báo thành công
                        MessageBox.Show("Đã đặt hàng thành công!");
                        RunDone = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ nếu có
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message);
            }
        }

        private void txtAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
