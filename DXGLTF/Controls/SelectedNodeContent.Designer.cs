namespace DXGLTF
{
    partial class SelectedNodeContent
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.scale = new DXGLTF.Controls.Vector3Control();
            this.euler = new DXGLTF.Controls.Vector3Control();
            this.translation = new DXGLTF.Controls.Vector3Control();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "T";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "R";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "S";
            // 
            // scale
            // 
            this.scale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scale.Location = new System.Drawing.Point(31, 62);
            this.scale.Name = "scale";
            this.scale.Size = new System.Drawing.Size(241, 19);
            this.scale.TabIndex = 4;
            // 
            // euler
            // 
            this.euler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.euler.Location = new System.Drawing.Point(31, 37);
            this.euler.Name = "euler";
            this.euler.Size = new System.Drawing.Size(241, 19);
            this.euler.TabIndex = 2;
            // 
            // translation
            // 
            this.translation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.translation.Location = new System.Drawing.Point(30, 12);
            this.translation.Name = "translation";
            this.translation.Size = new System.Drawing.Size(242, 19);
            this.translation.TabIndex = 0;
            // 
            // SelectedNodeContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.scale);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.euler);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.translation);
            this.Name = "SelectedNodeContent";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.Vector3Control translation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private Controls.Vector3Control euler;
        private System.Windows.Forms.Label label3;
        private Controls.Vector3Control scale;
    }
}
