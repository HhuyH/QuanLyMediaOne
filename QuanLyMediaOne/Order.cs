using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static QuanLyLinhKienDIenTu.Login;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QuanLyLinhKienDIenTu
{
    public partial class Order : Form
    {
        string strCon = Connecting.GetConnectionString();
        public MainForm mf;
        public static string productid;
        public static bool isAddMode;
        string role = UserSession.role;
        int UserID = UserSession.UserId;
        int IntproductId;
        int CusId;

        public Order(MainForm mainForm)
        {
            InitializeComponent();
            BackGroundColor();
            Round.SetSharpCornerPanel(panelAddCart);
            Round.SetSharpCornerPanel(panel6);
            Round.SetRoundedButton(btnCart);
            Round.SetSharpCornerPanel(panel2);
            mf = mainForm;
        }

        private void TxtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Kiểm tra xem ký tự được nhập vào có phải là chữ không
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                // Nếu là chữ, ngăn việc thêm ký tự đó vào TextBox
                e.Handled = true;
            }
        }
         
        private void Order_Load(object sender, EventArgs e)
        {
            LoadCustomersToComboBox();
            this.ControlBox = false;
            cboDanhMuc.SelectedIndexChanged += CboLinhKien_SelectedIndexChanged;
            LoadComponentTypes();
            LoadProductData();
            DataGrindview();
            cboCus.TextChanged += CboCus_TextChanged;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            btnCart.BackColor = panel2.BackColor;
            if (role == "Khach Hang")
            {
                tableLayoutPanel2.Visible = false;
            }
            else
            {
                panel6.Visible = false;
            }
            Cart.CustomerID = 0;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                // Kiểm tra xem có hàng được chọn không
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    // Lấy dòng được chọn đầu tiên
                    DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                    // Kiểm tra xem cột "product_id" có tồn tại trong DataGridView không
                    if (dataGridView1.Columns.Contains("MASANPHAM") && selectedRow.Cells["MASANPHAM"] != null && selectedRow.Cells["MASANPHAM"].Value != null)
                    {
                        // Gán giá trị của cột "product_id" cho biến AddSanPham.productid (kiểu string)
                        AddSanPham.productid = selectedRow.Cells["MASANPHAM"].Value.ToString();
                        AddSanPham.isViewMode = true;
                        // Mở cửa sổ AddSanPham
                        mf.OpenChildForm(new AddSanPham(mf));
                    }
                }
            }
        }


        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Kiểm tra xem có cell nào được chọn không
            if (dataGridView1.SelectedCells.Count > 0)
            {
                // Lấy cell đầu tiên được chọn
                DataGridViewCell selectedCell = dataGridView1.SelectedCells[0];

                // Kiểm tra xem cột "product_id" có tồn tại trong DataGridView không
                if (dataGridView1.Columns.Contains("MASANPHAM") && selectedCell.OwningRow.Cells["MASANPHAM"] != null && selectedCell.OwningRow.Cells["MASANPHAM"].Value != null)
                {
                    // Lấy ID sản phẩm từ cột "product_id"
                    string productId = selectedCell.OwningRow.Cells["MASANPHAM"].Value.ToString();

                    // Gọi phương thức GetProductID và chuyển ID sản phẩm vào form Cart
                    Cart.ProductID = GetProductID(productId);
                    IntproductId = GetProductID(productId);
                }
            }

        }

        //Lấy ID của product và chuyển nó vào form Cart
        public int GetProductID(string productID)
        {
            int productId = -1; // Giá trị mặc định nếu không tìm thấy sản phẩm

            using (SqlConnection connection = new SqlConnection(strCon))
            {
                string query = "SELECT MASANPHAM " +
                               "FROM SANPHAM " +
                               "WHERE MASANPHAM = @productid";
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandType = CommandType.Text;

                command.Parameters.AddWithValue("@productid", productID);

                connection.Open();
                object result = command.ExecuteScalar(); // Lấy giá trị trả về từ truy vấn
                if (result != null && result != DBNull.Value)
                {
                    productId = Convert.ToInt32(result);
                }
            }

            return productId;
        }


        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // Lấy nội dung tìm kiếm từ TextBox
            string keyword = txtSearch.Text.Trim();

            // Gọi phương thức SearchItems với từ khóa tìm kiếm
            Search(keyword);
        }

        private void Search(string keyword)
        {
            // Tạo câu truy vấn SQL tìm kiếm
            string query = "SELECT S.MASANPHAM, S.TENSANPHAM, S.GIA, S.SOLUONG, DM.TENDANHMUC " +
                                "FROM SANPHAM S INNER JOIN DANHMUCSANPHAM DM ON S.MADANHMUC = DM.MADANHMUC";

            // Tạo kết nối đến cơ sở dữ liệu
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                // Mở kết nối
                connection.Open();

                // Tạo đối tượng SqlCommand
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Thêm tham số cho từ khóa tìm kiếm
                    command.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                    // Tạo đối tượng SqlDataAdapter để lấy dữ liệu từ SQL Server và đổ vào DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        // Khởi tạo DataTable để lưu trữ dữ liệu
                        DataTable dt = new DataTable();

                        // Đổ dữ liệu từ SqlDataAdapter vào DataTable
                        adapter.Fill(dt);

                        // Đặt DataTable làm nguồn dữ liệu cho DataGridView
                        dataGridView1.DataSource = dt;
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
            MainForm mf = new MainForm();
            dataGridView1.BackgroundColor = mf.GetPanelBodyColor();
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.RowHeadersVisible = false;
            this.BackColor = mf.GetPanelBodyColor();
        }

        //chỉnh màu nền
        private void BackGroundColor()
        {
            MainForm mf = new MainForm();

            this.BackColor = mf.GetPanelBodyColor();
        }

        //tải sản phẩm
        private void LoadProductData()
        {
            // Sử dụng câu lệnh using để đảm bảo rằng SqlConnection được giải phóng đúng cách ngay cả khi có ngoại lệ xảy ra
            using (SqlConnection con = new SqlConnection(strCon))
            {
                try
                {
                    // Mở kết nối
                    con.Open();
                    // Câu truy vấn SQL để lấy dữ liệu từ bảng ProductInfo
                    string query = "SELECT S.MASANPHAM, S.TENSANPHAM, S.GIA, S.SOLUONG, DM.TENDANHMUC " +
                                "FROM SANPHAM S INNER JOIN DANHMUCSANPHAM DM ON S.MADANHMUC = DM.MADANHMUC";

                    // Tạo đối tượng SqlCommand
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Tạo đối tượng SqlDataAdapter
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            // Khởi tạo DataTable để lưu trữ dữ liệu
                            DataTable dt = new DataTable();

                            // Đổ dữ liệu từ SqlDataAdapter vào DataTable
                            adapter.Fill(dt);

                            // Đặt DataTable làm nguồn dữ liệu cho DataGridView
                            dataGridView1.DataSource = dt;

                            // Cấu hình các cột
                            dataGridView1.Columns["MASANPHAM"].HeaderText = "STT";
                            dataGridView1.Columns["TENSANPHAM"].HeaderText = "Tên sản phẩm";
                            dataGridView1.Columns["TENDANHMUC"].HeaderText = "Sản phẩm";
                            dataGridView1.Columns["SOLUONG"].HeaderText = "Số lượng";
                            dataGridView1.Columns["GIA"].HeaderText = "Giá";

                            // Căn giữa các cột
                            foreach (DataGridViewColumn column in dataGridView1.Columns)
                            {
                                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            }
                            dataGridView1.Columns["MASANPHAM"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi: " + ex.Message);
                }
            }
        }

        //Tải lên loại sản phảm
        private void LoadComponentTypes()
        {
            try
            {
                // Kết nối đến cơ sở dữ liệu
                using (SqlConnection conn = new SqlConnection(strCon))
                {
                    conn.Open();

                    // Thực hiện truy vấn và tải dữ liệu vào DataTable
                    string query = "SELECT MADANHMUC, TENDANHMUC FROM DANHMUCSANPHAM";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Thêm mục "Không" vào DataTable
                            DataRow row = dt.NewRow();
                            row["MADANHMUC"] = "0"; // Giá trị phù hợp cho MADANHMUC của mục "Không"
                            row["TENDANHMUC"] = "Không";
                            dt.Rows.InsertAt(row, 0); // Chèn vào vị trí đầu tiên của DataTable

                            // Gán dữ liệu vào ComboBox
                            cboDanhMuc.DataSource = dt;
                            cboDanhMuc.DisplayMember = "TENDANHMUC";
                            cboDanhMuc.ValueMember = "MADANHMUC";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CboLinhKien_SelectedIndexChanged(object sender, EventArgs e)
        {

            int SelectType = 0;
            if (SelectType != 0)
            {
                SelectType = cboDanhMuc.SelectedIndex;
                LoadProductsByCatagory(0);
            }
        }

        private void LoadProductsByCatagory(int selectedCategoryId)
        {
            using (SqlConnection con = new SqlConnection(strCon))
            {
                try
                {
                    con.Open();
                    string query = "SELECT S.MASANPHAM, S.TENSANPHAM, S.GIA, S.SOLUONG, DM.TENDANHMUC " +
                                   "FROM SANPHAM S INNER JOIN DANHMUCSANPHAM DM ON S.MADANHMUC = DM.MADANHMUC " +
                                   "WHERE S.MADANHMUC = @CategoryId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@CategoryId", selectedCategoryId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                    }
                    if(selectedCategoryId == 0)
                    {
                        LoadProductData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi: " + ex.Message);
                }
            }
        }

        private void cboLinhKien_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Kiểm tra nếu SelectedValue không phải là DataRowView
            if (cboDanhMuc.SelectedValue is DataRowView)
            {
                return; // Nếu là DataRowView, thoát khỏi phương thức
            }

            // Lấy ID danh mục được chọn
            int selectedCategoryId = Convert.ToInt32(cboDanhMuc.SelectedValue);

            // Gọi phương thức để load sản phẩm theo danh mục
            LoadProductsByCatagory(selectedCategoryId);
        }

        // Tạo một danh sách để lưu thông tin khách hàng
        List<string> customerInfoList = new List<string>();

        public class Customer
        {
            public string CustomerID { get; set; }
            public int MAND { get; set; }
            public string FullName { get; set; }
            public int PhoneNumber { get; set; } 

            public override string ToString()
            {
                return $"{CustomerID} | {FullName} | {PhoneNumber}";
            }
        }

        private void LoadCustomersToComboBox()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(strCon))
                {
                    conn.Open();

                    string query = "SELECT MAKH, MAND, HO, TEN, SDT FROM KHACHHANG";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cboCus.Items.Clear();

                    while (reader.Read())
                    {
                        Customer customer = new Customer
                        {
                            CustomerID = reader["MAKH"].ToString(),
                            MAND = Convert.ToInt32(reader["MAND"]),
                            FullName = reader["HO"].ToString() + " " + reader["TEN"].ToString(),
                            PhoneNumber = Convert.ToInt32(reader["SDT"])
                        };

                        cboCus.Items.Add(customer);
                    }


                    cboCus.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    cboCus.AutoCompleteSource = AutoCompleteSource.CustomSource;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message);
            }
        }

        private void cboCus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCus.SelectedItem != null)
            {
                // Ép kiểu đối tượng được chọn về Customer
                Customer selectedCustomer = (Customer)cboCus.SelectedItem;

                // Lấy CustomerID từ đối tượng Customer và gán vào biến CusId
                CusId = selectedCustomer.MAND;
                // Gán CustomerID cho Cart
                Cart.CustomerID = CusId;
            }
        }

        private void CboCus_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(role == "Khach Hang")
            {
                CusId = GetCustomerIdByUserId(UserID);
                // Gọi phương thức InsertCart để thêm sản phẩm vào giỏ hàng và kiểm tra kết quả trả về
                if (InsertCart(IntproductId, CusId, 1))
                {
                    MessageBox.Show("Sản phẩm đã được thêm vào giỏ hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // Gọi phương thức InsertCart để thêm sản phẩm vào giỏ hàng và kiểm tra kết quả trả về
                if (InsertCart(IntproductId, CusId, 1))
                {
                    MessageBox.Show("Sản phẩm đã được thêm vào giỏ hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        //Lấy CustomerID từ UserID
        private int GetCustomerIdByUserId(int userId)
        {
            string query = "SELECT MAND FROM KHACHHANG WHERE MAND = @user_id";
            
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("không tìm thấy mã khách hàng.");
                            return -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                        return -1;
                    }
                }
            }
        }

        //Thêm sản phẩm vào Cart
        public bool InsertCart(int productId, int customerId, int quantity)
        {
            // Tạo kết nối với cơ sở dữ liệu
            using (SqlConnection connection = new SqlConnection(strCon))
            {
                // Tạo đối tượng SqlCommand để thực thi Stored Procedure
                using (SqlCommand command = new SqlCommand("InsertCart", connection))
                {
                    // Đặt kiểu của SqlCommand là StoredProcedure
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm các tham số cho Stored Procedure
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.Parameters.AddWithValue("@MAND", customerId);
                    command.Parameters.AddWithValue("@Quantity", quantity);

                    try
                    {
                        // Mở kết nối và thực thi câu lệnh
                        connection.Open();
                        command.ExecuteNonQuery();
                        return true; // Trả về true nếu thêm thành công
                    }
                    catch (Exception ex)
                    {
                        // Xử lý lỗi nếu có
                        MessageBox.Show("Vui lòng chọn khách hàng và sảng phẩm muốn thêm");
                        return false; // Trả về false nếu có lỗi
                    }
                }
            }
        }

        private void btnCart_Click(object sender, EventArgs e)
        {
            if(role == "Khach Hang")
            {
                Cart.CustomerID = GetCustomerIdByUserId(UserID);
                mf.OpenChildForm(new Cart(mf));
                mf.TitleLabel.Text = "Giỏ hàng";
            }
            else
            {
                if (Cart.CustomerID != default(int))
                {
                    mf.OpenChildForm(new Cart(mf));
                    mf.TitleLabel.Text = "Giỏ hàng";
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn khách hàng");
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            mf.OpenChildForm(new ViewOrder(mf));
        }
    }
}
