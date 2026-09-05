import Layout from "../components/Layout";

export default function ComingSoon({ title, description }) {
  return (
    <Layout title={title}>
      <div className="card flex flex-col items-center justify-center text-center py-20 px-6">
        <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-[#d9b96a]/20 to-[#c9a24b]/10 flex items-center justify-center mb-4">
          <svg viewBox="0 0 24 24" className="w-8 h-8 text-[#c9a24b]" fill="none" stroke="currentColor" strokeWidth="1.6">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 7v5l3 2" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
        </div>
        <h2 className="text-lg font-bold text-gray-900 font-display mb-1.5">{title}</h2>
        <p className="text-sm text-gray-500 max-w-sm">
          {description || "Tính năng này đang được phát triển và sẽ sớm ra mắt."}
        </p>
      </div>
    </Layout>
  );
}